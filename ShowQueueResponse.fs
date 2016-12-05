namespace TruthOrDareBot

open System

type ShowQueueAcknowledged = {
    Queue : Player list
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
