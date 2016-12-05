﻿namespace TruthOrDareBot

open System
open System.Collections.Generic
open System.Linq
open FSharp.Control.Reactive

type IGame =
    abstract member AddPlayer : AddPlayerRequest -> AddPlayerResponse
    abstract member RemovePlayer : RemovePlayerRequest -> RemovePlayerResponse
    abstract member ShowQueue : ShowQueueRequest -> ShowQueueResponse
    abstract member WhoseTurn : WhoseTurnRequest -> WhoseTurnResponse
    abstract member NextTurn : NextTurnRequest -> NextTurnResponse
    abstract member Reminders : IObservable<Reminder>

module Rando =
    let random = Random()

    let sample (ls : 'a list) =
        ls.[random.Next ls.Length]

    let shuffle ls =
        ls |> List.sortBy (fun e -> random.Next())

open Rando

module Replies =
    let addPlayerAcknowledged() =
        sample [
            "Roger that."
            "Alrighty!"
            "10-4, good buddy."
        ]

    let addPlayerRejected() =
        sample [
            "You're already in the game, dingus."
        ]

    let removePlayerSelfAcknowledged() =
        sample [
            "Aww, you're no fun."
            "Alright... Leave if you must."
        ]

    let removePlayerOtherAcknowledged() =
        sample [
            "Alright, I removed them, but I'm not happy about it."
            "For you, anything. *Poof!* They're gone."
        ]

    let removePlayerSelfNotInGameRejected() =
        sample [
            "You're not in the game, silly!"
        ]

    let removePlayerOtherNotInGameRejected() =
        sample [
            "Um, they're not in the game..."
        ]

    let removePlayerOtherNotModRejected() =
        sample [
            "Only mods can remove other players from the game."
        ]

    let stayedStoppedGameAcknowledged() =
        sample [
            "Still waiting for more players..."
        ]
    
    let stayedInProgressGameAcknowledged() =
        sample [
            "Game in progress."
        ]

    let justStartedGameAcknowledged() =
        sample [
            "Go time."
            "Alright chums, let's do this."
        ]

    let justStoppedGameAcknowledged() =
        sample [
            "Boo, the game is stopped. :("
        ]

    let waitingForGameStartAcknowledged() =
        sample [
            "Waiting for game to start."
        ]

    let showQueueRejected() =
        sample [
            "There is no queue to show!"
            "No game in progress."
            "The game is stopped."
        ]

    let whoseTurnRejected() =
        sample [
            "It's nobody's turn."
            "No game in progress."
            "The game is stopped."
        ]

    let justAdvancedQueueAcknowledged() =
        sample [
            "Just advanced queue."
        ]

    let justStartedQueueAcknowledged() =
        sample [
            "Just started queue."
        ]

    let justStoppedQueueAcknowledged() =
        sample [
            "Just stopped queue."
        ]

    let justShuffledQueueAcknowledged() =
        sample [
            "Just shuffled queue."
        ]

    let nextTurnAcknowledged() =
        sample [
            "Okey-dokey."
        ]

    let nextTurnGameStoppedRejected() =
        sample [
            "I can't advance the queue when there is no game in progress!"
        ]

    let nextTurnNotModOrCurrentPlayerRejected() =
        sample [
            "Only mods or current asker/answerer can advance the queue."
        ]

    let askerHasntSaidAnythingReminder() =
        sample [
            "You haven't said anything in a while... Thinking of a good dare?"
        ]

    let answererHasntSaidAnythingReminder() =
        sample [
            "You haven't said anything in a while... Are you still there?"
        ]

    let askerAutoSkippedReminder() =
        sample [
            "You haven't said anything in too long, so you have been skipped and removed from the game!"
        ]

    let answererAutoSkippedReminder() =
        sample [
            "You haven't said anything in too long, so you have been skipped and removed from the game!"
        ]

type Game(channelMessages : IObservable<Discord.MessageEventArgs>, minimumPlayers : int, reminderTimeSpan : TimeSpan, autoSkipTimeSpan : TimeSpan) =
    let mutable queue : Player list = []
    let mutable players : Set<Player> = Set.empty

    let mutable currentGameStatus =
        {
            StartStatus =
                {
                    StateType = GameStateType.Stopped
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

    let getNewGameStatus() : GameStatus =
        let (newGameState, gameStateTransitionType) =
            if players.Count >= minimumPlayers then
                let transition =
                    if currentGameStatus.StartStatus.StateType = GameStateType.InProgress then
                        None
                    else
                        Some GameStateTransitionType.JustStarted
                (GameStateType.InProgress, transition)
            else
                let transition =
                    if currentGameStatus.StartStatus.StateType = GameStateType.InProgress then
                        Some GameStateTransitionType.JustStopped
                    else
                        None
                (GameStateType.Stopped, transition)

        let gameStateTransitionAcknowledgment =
            match gameStateTransitionType with
            | Some GameStateTransitionType.JustStarted ->
                Replies.justStartedGameAcknowledged()
            | Some GameStateTransitionType.JustStopped ->
                Replies.justStoppedGameAcknowledged()
            | None ->
                match newGameState with
                | GameStateType.InProgress ->
                    Replies.stayedInProgressGameAcknowledged()
                | GameStateType.Stopped ->
                    Replies.stayedStoppedGameAcknowledged()

        queue <-
                match gameStateTransitionType with
                | Some GameStateTransitionType.JustStarted ->
                    players |> List.ofSeq |> shuffle
                | Some GameStateTransitionType.JustStopped ->
                    []
                | None ->
                    if queue.Length > 1 then
                        queue
                    else
                        players |> List.ofSeq |> shuffle

        let currentTurn =
            match newGameState with
            | GameStateType.InProgress ->
                Some {
                    CurrentAsker = queue.[0]
                    CurrentAnswerer = queue.[1]
                }
            | GameStateType.Stopped ->
                None

        let gameQueueTransition =
            match gameStateTransitionType with
            | Some GameStateTransitionType.JustStarted ->
                Some {
                    Type = GameQueueTransitionType.JustStarted
                    Acknowledgment = Replies.justStartedQueueAcknowledged()
                }
            | Some GameStateTransitionType.JustStopped ->
                Some {
                    Type = GameQueueTransitionType.JustStopped
                    Acknowledgment = Replies.justStoppedQueueAcknowledged()
                }
            | None ->
                if currentTurn = currentGameStatus.QueueStatus.CurrentTurn then
                    None
                else
                    if currentGameStatus.QueueStatus.Queue.Length = players.Count then
                        Some {
                            Type = GameQueueTransitionType.JustShuffled
                            Acknowledgment = Replies.justShuffledQueueAcknowledged()
                        }
                    else
                        Some {
                            Type = GameQueueTransitionType.JustAdvanced
                            Acknowledgment = Replies.justAdvancedQueueAcknowledged()
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

    let reminders = Event<Reminder>()
    let mutable subscriptions : IDisposable list = []

    let rec updateGameStatus() =
        currentGameStatus <- getNewGameStatus()

        match currentGameStatus.QueueStatus.Transition with
        | Some { Type = GameQueueTransitionType.JustStarted; Acknowledgment = acknowledgment }
        | Some { Type = GameQueueTransitionType.JustAdvanced; Acknowledgment = acknowledgment }
        | Some { Type = GameQueueTransitionType.JustShuffled; Acknowledgment = acknowledgment } ->
            let { CurrentAsker = currentAsker; CurrentAnswerer = currentAnswerer } = currentGameStatus.QueueStatus.CurrentTurn.Value;

            let currentAskerMessages = channelMessages |> Observable.filter (fun m -> m.User.Id = currentAsker)
            let currentAnswererMessages = channelMessages |> Observable.filter (fun m -> m.User.Id = currentAnswerer)

            for subscription in subscriptions do
                do subscription.Dispose()

            let reminderTimer = reminderTimeSpan |> Observable.timerSpan

            let askerReminderSubscription =
                reminderTimer
                |> Observable.takeUntilOther currentAskerMessages
                |> Observable.subscribe (fun _ ->
                    do updateGameStatus()
                    let reminder = Replies.askerHasntSaidAnythingReminder()
                    reminders.Trigger { Player = currentAsker; Reminder = reminder; GameStatus = currentGameStatus }
                )

            let answererReminderSubscription =
                reminderTimer
                |> Observable.takeUntilOther currentAnswererMessages
                |> Observable.subscribe (fun _ ->
                    do updateGameStatus()
                    let reminder = Replies.answererHasntSaidAnythingReminder()
                    reminders.Trigger { Player = currentAnswerer; Reminder = reminder; GameStatus = currentGameStatus }
                )

            let autoSkipTimer = autoSkipTimeSpan |> Observable.timerSpan

            let removePlayer playerToRemove =
                if players.Contains currentAnswerer then
                    players <- players.Remove playerToRemove

                    if queue |> List.contains playerToRemove then
                        queue <- queue |> List.except [playerToRemove]

                    do updateGameStatus()

            let askerAutoSkipSubscription =
                autoSkipTimer
                |> Observable.takeUntilOther currentAskerMessages
                |> Observable.subscribe (fun _ ->
                    removePlayer currentAsker
                    let reminder = Replies.askerAutoSkippedReminder()
                    reminders.Trigger { Player = currentAsker; Reminder = reminder; GameStatus = currentGameStatus }
                )

            let answererAutoSkipSubscription =
                autoSkipTimer
                |> Observable.takeUntilOther currentAnswererMessages
                |> Observable.subscribe (fun _ ->
                    removePlayer currentAnswerer
                    let reminder = Replies.answererAutoSkippedReminder()
                    reminders.Trigger { Player = currentAnswerer; Reminder = reminder; GameStatus = currentGameStatus }
                )

            subscriptions <-
                [
                    askerReminderSubscription
                    answererReminderSubscription
                    askerAutoSkipSubscription
                    answererAutoSkipSubscription
                ]
        | _ -> ()

    interface IGame with
        member this.Reminders = reminders.Publish |> Observable.asObservable

        member this.AddPlayer addPlayerRequest =
            let { AddPlayerRequest.RequestingPlayer = player } = addPlayerRequest

            let addPlayerStatus =
                if players.Contains player then
                    let rejection = Replies.addPlayerRejected()
                    AddPlayerStatus.AddPlayerRejected {
                        Player = player
                        Rejection = rejection
                    }
                else
                    players <- players.Add player

                    let addPlayerAcknowledgment = Replies.addPlayerAcknowledged()

                    do updateGameStatus()

                    AddPlayerStatus.AddPlayerAcknowledged {
                        Player = player
                        Acknowledgment = addPlayerAcknowledgment
                        GameStatus = currentGameStatus
                    }
            
            {
                AddPlayerStatus = addPlayerStatus
            }

        member this.RemovePlayer removePlayerRequest =
            let { RemovePlayerRequest.PlayerToRemove = playerToRemove; RequestingPlayer = requestingPlayer; RequestingPlayerIsMod = requestingPlayerIsMod } = removePlayerRequest

            let removePlayer acknowledged rejected =
                if players.Contains playerToRemove then
                    players <- players.Remove playerToRemove

                    if queue |> List.contains playerToRemove then
                        queue <- queue |> List.except [playerToRemove]

                    let removePlayerAcknowledgment = acknowledged()

                    do updateGameStatus()

                    RemovePlayerStatus.RemovePlayerAcknowledged {
                        Player = playerToRemove
                        Acknowledgment = removePlayerAcknowledgment
                        GameStatus = currentGameStatus
                    }
                else
                    let rejection = rejected()
                    RemovePlayerStatus.RemovePlayerRejected {
                        Player = playerToRemove
                        Rejection = rejection
                    }

            let removePlayerStatus =
                if requestingPlayer = playerToRemove then
                    removePlayer Replies.removePlayerSelfAcknowledged Replies.removePlayerSelfNotInGameRejected
                else
                    if requestingPlayerIsMod then
                        removePlayer Replies.removePlayerOtherAcknowledged Replies.removePlayerOtherNotInGameRejected
                    else
                        let rejection = Replies.removePlayerOtherNotModRejected()
                        RemovePlayerStatus.RemovePlayerRejected {
                            Player = playerToRemove;
                            Rejection = rejection
                        }
            
            {
                RemovePlayerStatus = removePlayerStatus
            }

        member this.ShowQueue showQueueRequest =
            match currentGameStatus.StartStatus.StateType with
            | GameStateType.InProgress ->
                {
                    ShowQueueStatus = ShowQueueStatus.ShowQueueAcknowledged {
                         QueueOrAcknowledgment = QueueOrAcknowledgment.Queue currentGameStatus.QueueStatus.Queue
                         WaitingPlayers = currentGameStatus.QueueStatus.WaitingPlayers
                     }
                }
            | GameStateType.Stopped ->
                if players.Count > 0 then
                    {
                        ShowQueueStatus = ShowQueueStatus.ShowQueueAcknowledged {
                            QueueOrAcknowledgment = QueueOrAcknowledgment.Acknowledgment (Replies.waitingForGameStartAcknowledged())
                            WaitingPlayers = currentGameStatus.QueueStatus.WaitingPlayers
                        }
                    }
                else
                    {
                        ShowQueueStatus = ShowQueueStatus.ShowQueueRejected {
                            Rejection = Replies.showQueueRejected()
                        }
                    }

        member this.WhoseTurn whoseTurnRequest =
            match currentGameStatus.QueueStatus.CurrentTurn with
            | Some currentTurn ->
                {
                    WhoseTurnStatus = WhoseTurnStatus.WhoseTurnAcknowledged {
                        CurrentTurn = currentTurn
                    }
                }
            | None ->
                {
                    WhoseTurnStatus = WhoseTurnStatus.WhoseTurnRejected {
                        Rejection = Replies.whoseTurnRejected()
                    }
                }

        member this.NextTurn nextTurnRequest =
            match currentGameStatus.StartStatus.StateType with
            | GameStateType.InProgress ->
                let { CurrentAsker = currentAsker; CurrentAnswerer = currentAnswerer } = currentGameStatus.QueueStatus.CurrentTurn.Value
                let player = nextTurnRequest.RequestingPlayer
                if nextTurnRequest.RequestingPlayerIsMod || player = currentAsker || player = currentAnswerer then
                    queue <- match queue with
                             | [] -> queue
                             | _ :: newQueue -> newQueue

                    do updateGameStatus()

                    {
                        NextTurnStatus = NextTurnStatus.NextTurnAcknowledged {
                            Acknowledgment = Replies.nextTurnAcknowledged()
                            GameStatus = currentGameStatus
                        }
                    }
                else
                    {
                        NextTurnStatus = NextTurnRejected {
                            Rejection = Replies.nextTurnNotModOrCurrentPlayerRejected()
                        }
                    }
            | GameStateType.Stopped ->
                {
                    NextTurnStatus = NextTurnStatus.NextTurnRejected {
                        Rejection = Replies.nextTurnGameStoppedRejected()
                    }
                }