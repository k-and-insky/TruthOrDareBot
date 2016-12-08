namespace TruthOrDareBot

open System

type IGame =
    abstract member AddPlayer : AddPlayerRequest -> AddPlayerResponse
    abstract member RemovePlayer : RemovePlayerRequest -> RemovePlayerResponse
    abstract member Updates : IObservable<GameUpdate>
