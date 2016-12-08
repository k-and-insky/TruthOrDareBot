namespace TruthOrDareBot

open System

type Reminder = {
    Player : Player
    Reminder : string
    GameStatus : OldGameStatus
}
