namespace TruthOrDareBot

open System

type RemovePlayerAcknowledged = {
    Player : Player
    Acknowledgment : string
    GameStatus : GameStatus
}

type RemovePlayerRejected = {
    Player : Player
    Rejection : string
}

type RemovePlayerStatus =
    | RemovePlayerAcknowledged of RemovePlayerAcknowledged
    | RemovePlayerRejected of RemovePlayerRejected

type RemovePlayerResponse = {
    RemovePlayerStatus : RemovePlayerStatus
}