namespace TruthOrDareBot

open System

type AddPlayerAcknowledged = {
    RequestingPlayer : Player
    Acknowledgment : string
}

type AddPlayerRejected = {
    RequestingPlayer : Player
    Rejection : string
}

type AddPlayerResponse =
    | AddPlayerAcknowledged of AddPlayerAcknowledged
    | AddPlayerRejected of AddPlayerRejected
