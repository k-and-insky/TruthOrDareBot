namespace TruthOrDareBot

open System

type GameStateType =
    | WaitingForPlayers
    | Active

//type GameQueueState = {
//}

type GameState = {
    Type : GameStateType
    CurrentTurn : GameTurn option
    Queue : Player list
    WaitingPlayers : Player list
    Players : Set<Player>
}
