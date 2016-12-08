namespace TruthOrDareBot.Test

open System
open System.Linq
open FSharp.Control.Reactive
open NUnit.Framework
open FsUnit
open TruthOrDareBot

open BaseTest

[<TestFixture>]
module AddPlayerSelfTest =
    [<SetUp>]
    let setUp () = BaseTest.setUp ()

    [<Test>]
    let AddPlayer1Acknowledges () =
        let acknowledgment = addPlayerSelfAcknowledged player1
        Replies.addPlayerAcknowledgments |> should contain acknowledgment

    [<Test>]
    let AddPlayer1AddsToPlayers () =
        let _, updates = addPlayerSelf player1
        let update = updates |> List.last
        update.State.Players |> should equal (Set.ofList [player1])

    [<Test>]
    let AddPlayer1AddsToWaitingPlayers () =
        let _, updates = addPlayerSelf player1
        let update = updates |> List.last
        update.State.WaitingPlayers |> should contain player1

    [<Test>]
    let AddPlayer1DoesNotAddToQueue () =
        let _, updates = addPlayerSelf player1
        let update = updates |> List.last
        update.State.Queue |> should be Empty

    [<Test>]
    let AddPlayer1InformsOfPlayerAddedWaitingPlayers () =
        let result, updates = addPlayerSelf player1
        updates |> should haveLength 1
        let update = updates |> List.last
        update.Type |> should equal (GameUpdateType.PlayersUpdate GamePlayersUpdateType.GamePlayerAdded)
        Replies.playerAddedWaitingUpdateDescriptions player1Name (minimumPlayers - 1) |> should contain update.Description

    [<Test>]
    let AddPlayers12AddsToPlayers () =
        let _, updates = addPlayersSelf [player1; player2]
        let update = updates |> List.last
        update.State.Players |> should equal (Set.ofList [player1; player2])

    [<Test>]
    let AddPlayers12AddsToWaitingPlayers () =
        let _, updates = addPlayersSelf [player1; player2]
        let update = updates |> List.last
        update.State.WaitingPlayers |> should equal [player1; player2]

    [<Test>]
    let AddPlayers12DoesNotAddToQueue () =
        let _, updates = addPlayersSelf [player1; player2]
        let update = updates |> List.last
        update.State.Queue |> should be Empty

    [<Test>]
    let AddPlayers12InformsOfPlayerAddedWaitingPlayers () =
        let result, updates = addPlayersSelf [player1; player2]
        updates |> should haveLength 2
        let update = updates |> List.last
        update.Type |> should equal (GameUpdateType.PlayersUpdate GamePlayersUpdateType.GamePlayerAdded)
        Replies.playerAddedWaitingUpdateDescriptions player2Name (minimumPlayers - 2) |> should contain update.Description

    [<Test>]
    let AddPlayers1234Acknowledges () =
        let results, updates = addPlayersSelf [player1; player2; player3; player4]
        for result in results do
            result |> addPlayerIsAcknowledged |> should be True

    [<Test>]
    let AddPlayers1234InformsOfGameStart () =
        let results, updates = addPlayersSelf [player1; player2; player3; player4]
        updates |> should haveLength 6
        let update = updates.[4]
        update.Type |> should equal (GameUpdateType.StateUpdate GameStateUpdateType.GameStarted)
        Replies.justStartedGameAcknowledgments |> should contain update.Description

    [<Test>]
    let AddPlayers1234InformsOfQueueStart () =
        let results, updates = addPlayersSelf [player1; player2; player3; player4]
        updates |> should haveLength 6
        let update = updates |> List.last
        update.Type |> should equal (GameUpdateType.QueueUpdate GameQueueUpdateType.GameQueueStarted)
        Replies.justStartedQueueAcknowledgments (mentionPlayer player1) (mentionPlayer player2) |> should contain update.Description

    [<Test>]
    let AddPlayers12345DoesNotAddToWaitingPlayers () =
        let results, updates = addPlayersSelf [player1; player2; player3; player4; player5]
        updates |> should haveLength 7
        let update = updates |> List.last
        update.State.WaitingPlayers |> should be Empty

    [<Test>]
    let AddPlayers12345AddsToQueue () =
        let results, updates = addPlayersSelf [player1; player2; player3; player4; player5]
        updates |> should haveLength 7
        let update = updates |> List.last
        update.State.Queue |> should contain player5

    [<Test>]
    let AddPlayers12345InformsOfPlayerAddedToActiveQueue () =
        let results, updates = addPlayersSelf [player1; player2; player3; player4; player5]
        updates |> should haveLength 7
        let update = updates |> List.last
        update.Type |> should equal (GameUpdateType.PlayersUpdate GamePlayersUpdateType.GamePlayerAdded)
        Replies.playerAddedToActiveQueueUpdateDescriptions player5Name |> should contain update.Description