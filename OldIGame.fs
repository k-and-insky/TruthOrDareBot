namespace TruthOrDareBot

open System

type OldIGame =
    abstract member OldAddPlayer : AddPlayerRequest -> OldAddPlayerResponse
    abstract member OldRemovePlayer : RemovePlayerRequest -> OldRemovePlayerResponse
    abstract member ShowQueue : ShowQueueRequest -> ShowQueueResponse
    abstract member WhoseTurn : WhoseTurnRequest -> WhoseTurnResponse
    abstract member NextTurn : NextTurnRequest -> NextTurnResponse
    abstract member Reminders : IObservable<Reminder>
