namespace TruthOrDareBot

open System

type GameStateType =
    | Stopped
    | InProgress

type GameStateTransitionType =
    | JustStarted
    | JustStopped


type GameStartStatus = {
    StateType : GameStateType
    TransitionType : GameStateTransitionType option
    Acknowledgment : string
}

type GameQueueTransitionType =
    | JustStarted
    | JustAdvanced
    | JustStopped
    | JustShuffled

type GameQueueTransition = {
    Type : GameQueueTransitionType
    Acknowledgment : string
}

type GameTurn = {
    CurrentAnswerer : Player
    CurrentAsker : Player
}

type GameQueueStatus = {
    Queue : Player list
    WaitingPlayers : Player list
    CurrentTurn: GameTurn option
    Transition : GameQueueTransition option
}

type GameStatus = {
    StartStatus : GameStartStatus
    QueueStatus : GameQueueStatus
}