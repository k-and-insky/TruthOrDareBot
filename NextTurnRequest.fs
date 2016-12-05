namespace TruthOrDareBot

open System

type NextTurnRequest = {
    RequestingPlayer : Player
    RequestingPlayerIsMod : bool
}
