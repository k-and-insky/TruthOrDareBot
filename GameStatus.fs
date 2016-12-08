namespace TruthOrDareBot

open System

type OldGameStateType =
    | Stopped
    | InProgress

type OldGameStateTransitionType =
    | JustStarted
    | JustStopped


type OldGameStartStatus = {
    StateType : OldGameStateType
    TransitionType : OldGameStateTransitionType option
    Acknowledgment : string
}

type OldGameQueueTransitionType =
    | JustStarted
    | JustAdvanced
    | JustStopped
    | JustShuffled

type OldGameQueueTransition = {
    Type : OldGameQueueTransitionType
    Acknowledgment : string
}

type OldGameQueueStatus = {
    Queue : Player list
    WaitingPlayers : Player list
    CurrentTurn: GameTurn option
    Transition : OldGameQueueTransition option
}

type OldGameStatus = {
    StartStatus : OldGameStartStatus
    QueueStatus : OldGameQueueStatus
}