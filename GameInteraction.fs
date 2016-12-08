namespace TruthOrDareBot

open System

type GameInteraction<'a> = {
    Result : 'a
    Updates : IObservable<GameUpdate>
}
