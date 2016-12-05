namespace TruthOrDareBot

open System

type GameState =
    | Stopped
    | InProgress

type GameStateTransition =
    | JustStarted
    | JustStopped
    | StayedInProgress
    | StayedStopped

type GameStartStatus = {
    State : GameState
    StateTransition : GameStateTransition
    Acknowledgment : string
}

type GameStatus = {
    StartStatus : GameStartStatus
    Queue : Player list
}