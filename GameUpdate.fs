namespace TruthOrDareBot

open System

type GameStateUpdateType =
    | GameStarted
    | GameStopped

type GamePlayersUpdateType =
    | GamePlayerAdded
    | GamePlayerRemoved

type GameQueueUpdateType =
    | GameQueueStarted
    | GameQueueAdvanced
    | GameQueueStopped
    | GameQueueShuffled

type GameUpdateType =
    | StateUpdate of GameStateUpdateType
    | PlayersUpdate of GamePlayersUpdateType
    | QueueUpdate of GameQueueUpdateType

type GameUpdate = {
    Type : GameUpdateType
    Description : string
    State : GameState
}
