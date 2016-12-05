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

type Game() =
    let queue = new List<Player>()

    let getGameState() =
        if queue.Count >= 2 then
            GameState.InProgress
        else
            GameState.Stopped

    let getGameStartStatus previousGameState =
        let (newGameState, gameStateTransition) =
            if queue.Count >= 2 then
                let transition =
                    if previousGameState = GameState.InProgress then
                        GameStateTransition.StayedInProgress
                    else
                        GameStateTransition.JustStarted
                (GameState.InProgress, transition)
            else
                let transition =
                    if previousGameState = GameState.InProgress then
                        GameStateTransition.JustStopped
                    else
                        GameStateTransition.StayedStopped
                (GameState.Stopped, transition)

        let gameStateTransitionAcknowledgment =
            match gameStateTransition with
            | GameStateTransition.JustStarted ->
                Replies.justStartedGameAcknowledged()
            | GameStateTransition.JustStopped ->
                Replies.justStoppedGameAcknowledged()
            | GameStateTransition.StayedInProgress ->
                Replies.stayedInProgressGameAcknowledged()
            | GameStateTransition.StayedStopped ->
                Replies.stayedStoppedGameAcknowledged()

        {
            State = newGameState
            StateTransition = gameStateTransition
            Acknowledgment = gameStateTransitionAcknowledgment
        }

    interface IGame with
        member this.AddPlayer addPlayerRequest =
            let { AddPlayerRequest.Player = player } = addPlayerRequest

            let previousGameState = getGameState()

            let addPlayerStatus =
                if queue.Contains player then
                    let rejection = Replies.addPlayerRejected()
                    AddPlayerStatus.AddPlayerRejected {
                        Player = player
                        Rejection = rejection
                    }
                else
                    queue.Add player

                    let addPlayerAcknowledgment = Replies.addPlayerAcknowledged()

                    let gameStartStatus = getGameStartStatus previousGameState

                    let gameStatus = {
                        StartStatus = gameStartStatus
                        Queue = List.ofSeq queue
                    }

                    AddPlayerStatus.AddPlayerAcknowledged {
                        Player = player
                        Acknowledgment = addPlayerAcknowledgment
                        GameStatus = gameStatus
                    }
            
            {
                AddPlayerStatus = addPlayerStatus
            }

        member this.RemovePlayer removePlayerRequest =
            let { RemovePlayerRequest.Player = player } = removePlayerRequest

            let previousGameState = getGameState()

            let removePlayerStatus =
                if queue.Remove player then
                    let removePlayerAcknowledgment = Replies.removePlayerAcknowledged()

                    let gameStartStatus = getGameStartStatus previousGameState

                    let gameStatus = {
                        StartStatus = gameStartStatus
                        Queue = List.ofSeq queue
                    }

                    RemovePlayerStatus.RemovePlayerAcknowledged {
                        Player = player
                        Acknowledgment = removePlayerAcknowledgment
                        GameStatus = gameStatus
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
            match getGameState() with
            | GameState.InProgress ->
                {
                    ShowQueueStatus = ShowQueueStatus.ShowQueueAcknowledged {
                         Queue = List.ofSeq queue
                     }
                }
            | GameState.Stopped ->
                {
                    ShowQueueStatus = ShowQueueStatus.ShowQueueRejected {
                        Rejection = Replies.showQueueRejected()
                    }
                }

        member this.WhoseTurn whoseTurnRequest =
            match getGameState() with
            | GameState.InProgress ->
                {
                    WhoseTurnStatus = WhoseTurnStatus.WhoseTurnAcknowledged {
                        CurrentPlayer = queue.[0]
                        CurrentAsker = queue.[1]
                    }
                }
            | GameState.Stopped ->
                {
                    WhoseTurnStatus = WhoseTurnStatus.WhoseTurnRejected {
                        Rejection = Replies.whoseTurnRejected()
                    }
                }