namespace TruthOrDareBot

open System
open System.Collections.Generic
open System.Linq
open FSharp.Control.Reactive

type Game(rando : IRando, channelMessages : IObservable<Message>, playerName : Player -> string, mentionPlayer : Player -> string, isMod: Player -> bool, minimumPlayers : int, reminderTimeSpan : TimeSpan, autoSkipTimeSpan : TimeSpan) =
    let mutable queue : Player list = []
    let mutable players : Set<Player> = Set.empty

    let mutable oldCurrentGameStatus =
        {
            StartStatus =
                {
                    StateType = OldGameStateType.Stopped
                    TransitionType = None
                    Acknowledgment = ""
                }
            QueueStatus =
                {
                    Queue = []
                    WaitingPlayers = []
                    CurrentTurn = None
                    Transition = None
                }
        }

    let oldGetNewGameStatus() : OldGameStatus =
        let (newGameState, gameStateTransitionType) =
            if players.Count >= minimumPlayers then
                let transition =
                    if oldCurrentGameStatus.StartStatus.StateType = OldGameStateType.InProgress then
                        None
                    else
                        Some OldGameStateTransitionType.JustStarted
                (OldGameStateType.InProgress, transition)
            else
                let transition =
                    if oldCurrentGameStatus.StartStatus.StateType = OldGameStateType.InProgress then
                        Some OldGameStateTransitionType.JustStopped
                    else
                        None
                (OldGameStateType.Stopped, transition)

        let gameStateTransitionAcknowledgment =
            match gameStateTransitionType with
            | Some OldGameStateTransitionType.JustStarted ->
                Replies.justStartedGameAcknowledged rando
            | Some OldGameStateTransitionType.JustStopped ->
                Replies.justStoppedGameAcknowledged rando
            | None ->
                match newGameState with
                | OldGameStateType.InProgress ->
                    Replies.stayedInProgressGameAcknowledged rando
                | OldGameStateType.Stopped ->
                    Replies.stayedStoppedGameAcknowledged rando

        queue <-
                match gameStateTransitionType with
                | Some OldGameStateTransitionType.JustStarted ->
                    players |> List.ofSeq |> rando.Shuffle
                | Some OldGameStateTransitionType.JustStopped ->
                    []
                | None ->
                    if queue.Length > 1 then
                        queue
                    else
                        players |> List.ofSeq |> rando.Shuffle

        let currentTurn =
            match newGameState with
            | OldGameStateType.InProgress ->
                Some {
                    Asker = queue.[0]
                    Answerer = queue.[1]
                }
            | OldGameStateType.Stopped ->
                None

        let gameQueueTransition =
            match gameStateTransitionType with
            | Some OldGameStateTransitionType.JustStarted ->
                Some {
                    Type = OldGameQueueTransitionType.JustStarted
                    Acknowledgment = Replies.justStartedQueueAcknowledged (mentionPlayer currentTurn.Value.Asker) (mentionPlayer currentTurn.Value.Answerer) rando
                }
            | Some OldGameStateTransitionType.JustStopped ->
                Some {
                    Type = OldGameQueueTransitionType.JustStopped
                    Acknowledgment = Replies.justStoppedQueueAcknowledged rando
                }
            | None ->
                if currentTurn = oldCurrentGameStatus.QueueStatus.CurrentTurn then
                    None
                else
                    if queue.Length = players.Count then
                        Some {
                            Type = OldGameQueueTransitionType.JustShuffled
                            Acknowledgment = Replies.justShuffledQueueAcknowledged rando
                        }
                    else
                        Some {
                            Type = OldGameQueueTransitionType.JustAdvanced
                            Acknowledgment = Replies.justAdvancedQueueAcknowledged rando
                        }

        let waitingPlayers = Set.ofList queue |> Set.difference players |> List.ofSeq

        {
            StartStatus =
                {
                    StateType = newGameState
                    TransitionType = gameStateTransitionType
                    Acknowledgment = gameStateTransitionAcknowledgment
                }
            QueueStatus =
                {
                    Queue = queue
                    WaitingPlayers = waitingPlayers
                    CurrentTurn = currentTurn
                    Transition = gameQueueTransition
                }
        }

    let oldReminders = Event<Reminder> ()
    let mutable oldSubscriptions : IDisposable list = []

    let rec oldUpdateGameStatus() =
        oldCurrentGameStatus <- oldGetNewGameStatus()

        match oldCurrentGameStatus.QueueStatus.Transition with
        | Some { Type = OldGameQueueTransitionType.JustStarted; Acknowledgment = acknowledgment }
        | Some { Type = OldGameQueueTransitionType.JustAdvanced; Acknowledgment = acknowledgment }
        | Some { Type = OldGameQueueTransitionType.JustShuffled; Acknowledgment = acknowledgment } ->
            let { Asker = currentAsker; Answerer = currentAnswerer } = oldCurrentGameStatus.QueueStatus.CurrentTurn.Value;

            let currentAskerMessages = channelMessages |> Observable.filter (fun m -> m.Sender = currentAsker)
            let currentAnswererMessages = channelMessages |> Observable.filter (fun m -> m.Sender = currentAnswerer)

            for subscription in oldSubscriptions do
                do subscription.Dispose()

            let reminderTimer = reminderTimeSpan |> Observable.timerSpan

            let askerReminderSubscription =
                reminderTimer
                |> Observable.takeUntilOther currentAskerMessages
                |> Observable.subscribe (fun _ ->
                    do oldUpdateGameStatus()
                    let reminder = Replies.askerHasntSaidAnythingReminder rando
                    oldReminders.Trigger { Player = currentAsker; Reminder = reminder; GameStatus = oldCurrentGameStatus }
                )

            let answererReminderSubscription =
                reminderTimer
                |> Observable.takeUntilOther currentAnswererMessages
                |> Observable.subscribe (fun _ ->
                    do oldUpdateGameStatus()
                    let reminder = Replies.answererHasntSaidAnythingReminder rando
                    oldReminders.Trigger { Player = currentAnswerer; Reminder = reminder; GameStatus = oldCurrentGameStatus }
                )

            let autoSkipTimer = autoSkipTimeSpan |> Observable.timerSpan

            let removePlayer playerToRemove =
                if players.Contains currentAnswerer then
                    players <- players.Remove playerToRemove

                    if queue |> List.contains playerToRemove then
                        queue <- queue |> List.except [playerToRemove]

                    do oldUpdateGameStatus()

            let askerAutoSkipSubscription =
                autoSkipTimer
                |> Observable.takeUntilOther currentAskerMessages
                |> Observable.subscribe (fun _ ->
                    removePlayer currentAsker
                    let reminder = Replies.askerAutoSkippedReminder rando
                    oldReminders.Trigger { Player = currentAsker; Reminder = reminder; GameStatus = oldCurrentGameStatus }
                )

            let answererAutoSkipSubscription =
                autoSkipTimer
                |> Observable.takeUntilOther currentAnswererMessages
                |> Observable.subscribe (fun _ ->
                    removePlayer currentAnswerer
                    let reminder = Replies.answererAutoSkippedReminder rando
                    oldReminders.Trigger { Player = currentAnswerer; Reminder = reminder; GameStatus = oldCurrentGameStatus }
                )

            oldSubscriptions <-
                [
                    askerReminderSubscription
                    answererReminderSubscription
                    askerAutoSkipSubscription
                    answererAutoSkipSubscription
                ]
        | _ -> ()

    let computeNewGameState () =
        let newGameStateType =
            if players.Count >= minimumPlayers then
                GameStateType.Active
            else
                GameStateType.WaitingForPlayers

        let newQueue =
            if newGameStateType = GameStateType.Active && queue.Length <= 1 then
                players |> List.ofSeq |> rando.Shuffle
            else
                queue

        let newCurrentTurn =
            match newGameStateType with
            | GameStateType.Active ->
                Some {
                    Asker = newQueue.[0]
                    Answerer = newQueue.[1]
                }
            | GameStateType.WaitingForPlayers ->
                None

        let newWaitingPlayers =
            match newGameStateType with
            | GameStateType.Active ->
                Set.difference players (Set.ofList newQueue)
            | GameStateType.WaitingForPlayers ->
                players
            |> List.ofSeq

        {
            Type = newGameStateType
            CurrentTurn = newCurrentTurn
            Queue = newQueue
            WaitingPlayers = newWaitingPlayers
            Players = players
        }

    let mutable currentGameState : GameState = computeNewGameState ()
    let updates = Event<GameUpdate> ()

    let updateGameState () =
        let previousGameState = currentGameState
        let newGameState = computeNewGameState ()
        currentGameState <- newGameState

        let newPlayers = Set.difference newGameState.Players previousGameState.Players

        for newPlayer in newPlayers do
            let description =
                match previousGameState.Type with
                | GameStateType.WaitingForPlayers ->
                    Replies.playerAddedWaitingUpdateDescription (playerName newPlayer) (minimumPlayers - players.Count)
                | GameStateType.Active ->
                    Replies.playerAddedToActiveQueueUpdateDescription (playerName newPlayer)
                    
            updates.Trigger
                {
                    Type = GameUpdateType.PlayersUpdate GamePlayersUpdateType.GamePlayerAdded
                    Description = description rando
                    State = newGameState
                }

        do match previousGameState.Type, newGameState.Type with
            | GameStateType.WaitingForPlayers, GameStateType.WaitingForPlayers ->
                ()
            | GameStateType.WaitingForPlayers, GameStateType.Active ->
                updates.Trigger
                    {
                        Type = GameUpdateType.StateUpdate GameStateUpdateType.GameStarted
                        Description = Replies.justStartedGameAcknowledged rando
                        State = newGameState
                    }
                updates.Trigger
                    {
                        Type = GameUpdateType.QueueUpdate GameQueueUpdateType.GameQueueStarted
                        Description = Replies.justStartedQueueAcknowledged (mentionPlayer newGameState.CurrentTurn.Value.Asker) (mentionPlayer newGameState.CurrentTurn.Value.Answerer) rando
                        State = newGameState
                    }
            | GameStateType.Active, GameStateType.WaitingForPlayers ->
                ()
            | GameStateType.Active, GameStateType.Active ->
                ()

    interface IGame with
        member this.AddPlayer addPlayerRequest =
            let { RequestingPlayer = requestingPlayer; PlayerToAdd = playerToAdd } = addPlayerRequest

            let addPlayer acknowledged rejected =
                if players.Contains playerToAdd then
                    let rejection = rejected rando

                    AddPlayerResponse.AddPlayerRejected {
                        RequestingPlayer = requestingPlayer
                        Rejection = rejection
                    }
                else
                    do players <- players.Add playerToAdd

                    do if currentGameState.Type = GameStateType.Active then
                        queue <- List.append queue [playerToAdd]

                    do updateGameState ()

                    let addPlayerAcknowledgment = acknowledged rando

                    AddPlayerResponse.AddPlayerAcknowledged {
                        RequestingPlayer = requestingPlayer
                        Acknowledgment = addPlayerAcknowledgment
                    }

            addPlayer Replies.addPlayerAcknowledged Replies.addPlayerRejected

        member this.RemovePlayer removePlayerRequest =
            let { RequestingPlayer = requestingPlayer; PlayerToRemove = playerToRemove } = removePlayerRequest

            let removePlayer acknowledged rejected =
                if players.Contains playerToRemove then
                    do players <- players.Remove playerToRemove

                    do if queue |> List.contains playerToRemove then
                        queue <- List.except [playerToRemove] queue

                    do updateGameState ()

                    RemovePlayerResponse.RemovePlayerAcknowledged {
                        RequestingPlayer = playerToRemove
                        Acknowledgment = acknowledged rando
                    }
                else
                    RemovePlayerResponse.RemovePlayerRejected {
                        RequestingPlayer = playerToRemove
                        Rejection = rejected rando
                    }

            if requestingPlayer = playerToRemove then
                removePlayer Replies.removePlayerSelfAcknowledged Replies.removePlayerSelfNotInGameRejected
            else
                if isMod requestingPlayer then
                    removePlayer Replies.removePlayerOtherAcknowledged Replies.removePlayerOtherNotInGameRejected
                else
                    RemovePlayerResponse.RemovePlayerRejected {
                        RequestingPlayer = requestingPlayer
                        Rejection = Replies.removePlayerOtherNotModRejected rando
                    }

        member this.Updates = updates.Publish |> Observable.asObservable

    interface OldIGame with
        member this.Reminders = oldReminders.Publish |> Observable.asObservable

        member this.OldAddPlayer addPlayerRequest =
            let { AddPlayerRequest.RequestingPlayer = player } = addPlayerRequest

            let addPlayerStatus =
                if players.Contains player then
                    let rejection = Replies.addPlayerRejected rando
                    OldAddPlayerStatus.AddPlayerRejected {
                        Player = player
                        Rejection = rejection
                    }
                else
                    players <- players.Add player

                    let addPlayerAcknowledgment = Replies.addPlayerAcknowledged rando

                    do oldUpdateGameStatus()

                    OldAddPlayerStatus.AddPlayerAcknowledged {
                        Player = player
                        Acknowledgment = addPlayerAcknowledgment
                        GameStatus = oldCurrentGameStatus
                    }
            
            {
                AddPlayerStatus = addPlayerStatus
            }

        member this.OldRemovePlayer removePlayerRequest =
            let { RemovePlayerRequest.PlayerToRemove = playerToRemove; RequestingPlayer = requestingPlayer } = removePlayerRequest

            let removePlayer acknowledged rejected =
                if players.Contains playerToRemove then
                    players <- players.Remove playerToRemove

                    if queue |> List.contains playerToRemove then
                        queue <- queue |> List.except [playerToRemove]

                    let removePlayerAcknowledgment = acknowledged rando

                    do oldUpdateGameStatus()

                    OldRemovePlayerStatus.RemovePlayerAcknowledged {
                        Player = playerToRemove
                        Acknowledgment = removePlayerAcknowledgment
                        GameStatus = oldCurrentGameStatus
                    }
                else
                    let rejection = rejected rando
                    OldRemovePlayerStatus.RemovePlayerRejected {
                        Player = playerToRemove
                        Rejection = rejection
                    }

            let removePlayerStatus =
                if requestingPlayer = playerToRemove then
                    removePlayer Replies.removePlayerSelfAcknowledged Replies.removePlayerSelfNotInGameRejected
                else
                    if isMod requestingPlayer then
                        removePlayer Replies.removePlayerOtherAcknowledged Replies.removePlayerOtherNotInGameRejected
                    else
                        let rejection = Replies.removePlayerOtherNotModRejected rando
                        OldRemovePlayerStatus.RemovePlayerRejected {
                            Player = playerToRemove;
                            Rejection = rejection
                        }
            
            {
                RemovePlayerStatus = removePlayerStatus
            }

        member this.ShowQueue showQueueRequest =
            match oldCurrentGameStatus.StartStatus.StateType with
            | OldGameStateType.InProgress ->
                {
                    ShowQueueStatus = ShowQueueStatus.ShowQueueAcknowledged {
                         QueueOrAcknowledgment = QueueOrAcknowledgment.Queue oldCurrentGameStatus.QueueStatus.Queue
                         WaitingPlayers = oldCurrentGameStatus.QueueStatus.WaitingPlayers
                     }
                }
            | OldGameStateType.Stopped ->
                if players.Count > 0 then
                    {
                        ShowQueueStatus = ShowQueueStatus.ShowQueueAcknowledged {
                            QueueOrAcknowledgment = QueueOrAcknowledgment.Acknowledgment (Replies.waitingForGameStartAcknowledged rando)
                            WaitingPlayers = oldCurrentGameStatus.QueueStatus.WaitingPlayers
                        }
                    }
                else
                    {
                        ShowQueueStatus = ShowQueueStatus.ShowQueueRejected {
                            Rejection = Replies.showQueueGameStoppedAcknowledged rando
                        }
                    }

        member this.WhoseTurn whoseTurnRequest =
            match oldCurrentGameStatus.QueueStatus.CurrentTurn with
            | Some currentTurn ->
                {
                    WhoseTurnStatus = WhoseTurnStatus.WhoseTurnAcknowledged {
                        CurrentTurn = currentTurn
                    }
                }
            | None ->
                {
                    WhoseTurnStatus = WhoseTurnStatus.WhoseTurnRejected {
                        Rejection = Replies.whoseTurnRejected rando
                    }
                }

        member this.NextTurn nextTurnRequest =
            match oldCurrentGameStatus.StartStatus.StateType with
            | OldGameStateType.InProgress ->
                let { Asker = currentAsker; Answerer = currentAnswerer } = oldCurrentGameStatus.QueueStatus.CurrentTurn.Value
                let player = nextTurnRequest.RequestingPlayer
                if nextTurnRequest.RequestingPlayerIsMod || player = currentAsker || player = currentAnswerer then
                    queue <- match queue with
                             | [] -> queue
                             | _ :: newQueue -> newQueue

                    do oldUpdateGameStatus()

                    {
                        NextTurnStatus = NextTurnStatus.NextTurnAcknowledged {
                            Acknowledgment = Replies.nextTurnAcknowledged rando
                            GameStatus = oldCurrentGameStatus
                        }
                    }
                else
                    {
                        NextTurnStatus = NextTurnRejected {
                            Rejection = Replies.nextTurnNotModOrCurrentPlayerRejected rando
                        }
                    }
            | OldGameStateType.Stopped ->
                {
                    NextTurnStatus = NextTurnStatus.NextTurnRejected {
                        Rejection = Replies.nextTurnGameStoppedRejected rando
                    }
                }