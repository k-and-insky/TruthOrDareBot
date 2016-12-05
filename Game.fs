namespace TruthOrDareBot

open System
open System.Collections.Generic

type IGame =
    abstract member AddPlayer : AddPlayerRequest -> AddPlayerResponse
    abstract member RemovePlayer : RemovePlayerRequest -> RemovePlayerResponse
    abstract member ShowQueue : ShowQueueRequest -> ShowQueueResponse
    abstract member WhoseTurn : WhoseTurnRequest -> WhoseTurnResponse

module Replies =
    let sample (ls : 'a list) =
        ls.[Random().Next ls.Length]

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

type Game() =
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

    let shuffle (players : Player list) : Player list =
        players

    let getGameStatus() : GameStatus =
        do Console.WriteLine "getGameStatus"

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
        
        do Console.WriteLine "computed new game state"

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
        
        do Console.WriteLine "computed transition acknowledgment"

        let queue =
            match gameStateTransitionType with
            | Some GameStateTransitionType.JustStarted ->
                players |> List.ofSeq |> shuffle
            | Some GameStateTransitionType.JustStopped ->
                []
            | None ->
                currentGameStatus.QueueStatus.Queue

        do Console.WriteLine "computed queue"

        let currentTurn =
            match gameStateTransitionType with
            | None ->
                currentGameStatus.QueueStatus.CurrentTurn
            | Some GameStateTransitionType.JustStarted ->
                Some {
                    CurrentAnswerer = queue.[0]
                    CurrentAsker = queue.[1]
                }
            | Some GameStateTransitionType.JustStopped ->
                None

        do Console.WriteLine "computed currentTurn"

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

        do Console.WriteLine "computed gameQueueTransition"

        let waitingPlayers = Set.ofList queue |> Set.difference players |> List.ofSeq

        do Console.WriteLine "computed waitingPlayers"

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

                    currentGameStatus <- getGameStatus()

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

                    currentGameStatus <- getGameStatus()

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