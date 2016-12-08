namespace TruthOrDareBot

open System

type OldRemovePlayerAcknowledged = {
    Player : Player
    Acknowledgment : string
    GameStatus : OldGameStatus
}

type OldRemovePlayerRejected = {
    Player : Player
    Rejection : string
}

type OldRemovePlayerStatus =
    | RemovePlayerAcknowledged of OldRemovePlayerAcknowledged
    | RemovePlayerRejected of OldRemovePlayerRejected

type OldRemovePlayerResponse = {
    RemovePlayerStatus : OldRemovePlayerStatus
}
