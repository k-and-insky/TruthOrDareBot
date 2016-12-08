namespace TruthOrDareBot

open System

type RemovePlayerAcknowledged = {
    RequestingPlayer : Player
    Acknowledgment : string
}

type RemovePlayerRejected = {
    RequestingPlayer : Player
    Rejection : string
}

type RemovePlayerResponse =
    | RemovePlayerAcknowledged of RemovePlayerAcknowledged
    | RemovePlayerRejected of RemovePlayerRejected
