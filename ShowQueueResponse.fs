namespace TruthOrDareBot

open System

type QueueOrAcknowledgment =
    | Queue of Player list
    | Acknowledgment of string

type ShowQueueAcknowledged = {
    QueueOrAcknowledgment : QueueOrAcknowledgment
    WaitingPlayers : Player list
}

type ShowQueueRejected = {
    Rejection : string
}

type ShowQueueStatus =
    | ShowQueueAcknowledged of ShowQueueAcknowledged
    | ShowQueueRejected of ShowQueueRejected

type ShowQueueResponse = {
    ShowQueueStatus : ShowQueueStatus
}
