namespace TruthOrDareBot

open System

type NextTurnAcknowledged = {
    Acknowledgment : string
    GameStatus : OldGameStatus
}

type NextTurnRejected = {
    Rejection : string
}

type NextTurnStatus =
    | NextTurnAcknowledged of NextTurnAcknowledged
    | NextTurnRejected of NextTurnRejected

type NextTurnResponse = {
    NextTurnStatus : NextTurnStatus
}
