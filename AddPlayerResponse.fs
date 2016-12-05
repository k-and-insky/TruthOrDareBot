namespace TruthOrDareBot

open System

type AddPlayerAcknowledged = {
    Player : Player
    Acknowledgment : string
    GameStatus : GameStatus
}

type AddPlayerRejected = {
    Player : Player
    Rejection : string
}

type AddPlayerStatus =
    | AddPlayerAcknowledged of AddPlayerAcknowledged
    | AddPlayerRejected of AddPlayerRejected

type AddPlayerResponse = {
    AddPlayerStatus : AddPlayerStatus
}