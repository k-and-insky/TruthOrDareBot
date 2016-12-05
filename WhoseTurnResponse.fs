namespace TruthOrDareBot

open System

type WhoseTurnAcknowledged = {
    CurrentAsker : Player
    CurrentPlayer : Player
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
