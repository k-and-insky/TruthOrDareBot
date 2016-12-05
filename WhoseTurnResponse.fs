namespace TruthOrDareBot

open System

type WhoseTurnAcknowledged = {
    CurrentTurn : GameTurn
}

type WhoseTurnRejected = {
    Rejection : string
}

type WhoseTurnStatus =
    | WhoseTurnAcknowledged of WhoseTurnAcknowledged
    | WhoseTurnRejected of WhoseTurnRejected

type WhoseTurnResponse = {
    WhoseTurnStatus : WhoseTurnStatus
}
