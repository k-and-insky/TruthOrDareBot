namespace TruthOrDareBot

open System

type OldAddPlayerAcknowledged = {
    Player : Player
    Acknowledgment : string
    GameStatus : OldGameStatus
}

type OldAddPlayerRejected = {
    Player : Player
    Rejection : string
}

type OldAddPlayerStatus =
    | AddPlayerAcknowledged of OldAddPlayerAcknowledged
    | AddPlayerRejected of OldAddPlayerRejected

type OldAddPlayerResponse = {
    AddPlayerStatus : OldAddPlayerStatus
}