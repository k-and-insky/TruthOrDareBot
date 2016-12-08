namespace TruthOrDareBot.Test

open System
open System.Linq
open FSharp.Control.Reactive
open NUnit.Framework
open FsUnit
open TruthOrDareBot

open BaseTest

[<TestFixture>]
module RemovePlayerTest =
    [<SetUp>]
    let setUp () = BaseTest.setUp ()

    [<Test>]
    let ``Remove player1 acknowledges`` () =
        do addPlayerSelf player1 |> ignore
        let acknowledgment = removePlayerAcknowledged player1
        Replies.removePlayerSelfAcknowledgments |> should contain acknowledgment

    [<Test>]
    let ``Add [player1; player2; player3; player4] and remove player1 informs of game start`` () =
        do addPlayersSelf [player1; player2; player3; player4] |> ignore
        let acknowledgment = removePlayerAcknowledged player1
        Replies.removePlayerSelfAcknowledgments |> should contain acknowledgment
