namespace TruthOrDareBot

open System
open System.Collections.Generic

type IGame =
    abstract member AddPlayer : AddPlayerRequest -> AddPlayerResponse
    abstract member RemovePlayer : RemovePlayerRequest -> RemovePlayerResponse
    abstract member ShowQueue : ShowQueueRequest -> ShowQueueResponse
    abstract member WhoseTurn : WhoseTurnRequest -> WhoseTurnResponse
    abstract member NextTurn : NextTurnRequest -> NextTurnResponse

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
            "You're already in the queue, dingus."
        ]

    let removePlayerAcknowledged() =
        sample [
            "Aww, you're no fun."
            "Alright... Leave if you must."
        ]

    let removePlayerRejected() =
        sample [
            "You're not in the queue, silly!"
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

type Game() =
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

    let getGameStatus() : GameStatus =
        let (newGameState, gameStateTransitionType) =
            if players.Count >= 2 then
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

    let updateGameStatus() =
        currentGameStatus <- getGameStatus()

    interface IGame with
        member this.AddPlayer addPlayerRequest =
            let { AddPlayerRequest.Player = player } = addPlayerRequest

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
            let { RemovePlayerRequest.Player = player } = removePlayerRequest

            let removePlayerStatus =
                if players.Contains player then
                    players <- players.Remove player

                    let removePlayerAcknowledgment = Replies.removePlayerAcknowledged()

                    do updateGameStatus()

                    RemovePlayerStatus.RemovePlayerAcknowledged {
                        Player = player
                        Acknowledgment = removePlayerAcknowledgment
                        GameStatus = currentGameStatus
                    }
                else
                    let rejection = Replies.removePlayerRejected()
                    RemovePlayerStatus.RemovePlayerRejected {
                        Player = player
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
                queue <- match queue with
                         | [] -> queue
                         | _ :: newQueue -> newQueue

                do updateGameStatus()

                {
                    NextTurnStatus = NextTurnStatus.NextTurnAcknowledged {
                        Acknowledgment = Replies.nextTurnAcknowledged()
                        CurrentTurn = currentGameStatus.QueueStatus.CurrentTurn.Value
                    }
                }
            | GameStateType.Stopped ->
                {
                    NextTurnStatus = NextTurnStatus.NextTurnRejected {
                        Rejection = Replies.nextTurnGameStoppedRejected()
                    }
                }