namespace TruthOrDareBot.Test

open System
open FSharp.Control.Reactive
open NUnit.Framework
open FsUnit
open TruthOrDareBot

[<TestFixture>]
type GameTest() =
    let channelMessages = Event<Message>()
    let minimumPlayers = 4
    let game = Game (channelMessages.Publish |> Observable.asObservable, minimumPlayers, TimeSpan(0, 0, 1), TimeSpan(0, 0, 2)) :> IGame

    let player1 : Player = 1UL
    let player2 : Player = 2UL
    let player3 : Player = 3UL
    let player4 : Player = 4UL

    let addPlayer player =
        match (game.AddPlayer { RequestingPlayer = player }).AddPlayerStatus with
        | AddPlayerAcknowledged a -> a
        | AddPlayerRejected r -> raise (Exception "Rejected")

    [<Test>]
    member this.AddPlayer1InformsOfBeingAdded () =
        let { AddPlayerAcknowledged.Player = addedPlayer; Acknowledgment = acknowledgment } = addPlayer player1
        addedPlayer |> should equal player1
        Replies.addPlayerAcknowledgments |> should contain acknowledgment