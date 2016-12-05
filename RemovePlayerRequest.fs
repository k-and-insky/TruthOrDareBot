namespace TruthOrDareBot

open System

type RemovePlayerRequest = {
    RequestingPlayer : Player
    PlayerToRemove : Player
    RequestingPlayerIsMod : bool
}
