namespace TruthOrDareBot.Test

open System
open System.Linq
open FSharp.Control.Reactive
open NUnit.Framework
open TruthOrDareBot

module BaseTest =
    let rando =
        {
            new IRando with
                member this.Sample ls =
                    ls.First ()

                member this.Shuffle ls =
                    ls
        }

    let channelMessages = Event<Message>()
    let minimumPlayers = 4
    let reminderTimeSpan = TimeSpan (0, 0, 1)
    let autoSkipTimeSpan = TimeSpan (0, 0, 2)

    let player1 : Player = 1UL
    let player2 : Player = 2UL
    let player3 : Player = 3UL
    let player4 : Player = 4UL
    let player5 : Player = 5UL

    let playerName player = sprintf "Player %u" player
    let mentionPlayer player = sprintf "@player%u" player

    let player1Name = playerName player1
    let player2Name = playerName player2
    let player3Name = playerName player3
    let player4Name = playerName player4
    let player5Name = playerName player5

    let isMod _ = false

    let mutable game : IGame option = None

    let interact f =
        let mutable updates : GameUpdate list = []
        use subscription = game.Value.Updates |> Observable.subscribe (fun u ->
                updates <- List.append updates [u]
            )
        let result = f ()
        result, updates

    let addPlayerSelf player = game.Value |> Interaction.addPlayerSelf player

    let addPlayersSelf players =
        game.Value |> Interaction.interact (fun g -> players |> List.map (fun player -> g.AddPlayer { RequestingPlayer = player; PlayerToAdd = player }))

    let addPlayerIsAcknowledged response =
        match response with
        | AddPlayerResponse.AddPlayerAcknowledged _ -> true
        | AddPlayerResponse.AddPlayerRejected _ -> false

    let addPlayerSelfAcknowledged player =
        let response, _ = addPlayerSelf player
        match response with
        | AddPlayerResponse.AddPlayerAcknowledged a -> a.Acknowledgment
        | AddPlayerResponse.AddPlayerRejected r -> raise (Exception r.Rejection)

    let removePlayerSelf player = game.Value |> Interaction.removePlayerSelf player

    let removePlayersSelf players =
        game.Value |> Interaction.interact (fun g -> players |> List.map (fun player -> g.RemovePlayer { RequestingPlayer = player; PlayerToRemove = player }))

    let removePlayerSelfIsAcknowledged response =
        match response with
        | RemovePlayerResponse.RemovePlayerAcknowledged _ -> true
        | RemovePlayerResponse.RemovePlayerRejected _ -> false

    let removePlayerAcknowledged player =
        let response, _ = removePlayerSelf player
        match response with
        | RemovePlayerResponse.RemovePlayerAcknowledged a -> a.Acknowledgment
        | RemovePlayerResponse.RemovePlayerRejected r -> raise (Exception r.Rejection)

    let setUp () =
        game <- Some (Game
            (
                rando,
                channelMessages.Publish |> Observable.asObservable,
                playerName,
                mentionPlayer,
                isMod,
                minimumPlayers,
                reminderTimeSpan,
                autoSkipTimeSpan
            ) :> IGame)


//type BaseTest () =
//    let channelMessages = Event<Message>()
//    let minimumPlayers = 4
//    let reminderTimeSpan = TimeSpan (0, 0, 1)
//    let autoSkipTimeSpan = TimeSpan (0, 0, 2)

//    let player1 : Player = 1UL
//    let player2 : Player = 2UL
//    let player3 : Player = 3UL
//    let player4 : Player = 4UL
//    let player5 : Player = 5UL

//    let playerName player = sprintf "Player %u" player

//    let player1Name = this.playerName this.player1
//    let player2Name = this.playerName this.player2
//    let player3Name = this.playerName this.player3
//    let player4Name = this.playerName this.player4
//    let player5Name = this.playerName this.player5

//    let isMod _ = false

//    member val game.Value' : Igame.Value option = None with get, set
//    let game.Value = this.game.Value'.Value

//    let interact f =
//        let mutable updates : game.ValueUpdate list = []
//        use subscription = this.game.Value.Updates |> Observable.subscribe (fun u ->
//                updates <- List.append updates [u]
//            )
//        let result = f ()
//        result, updates

//    let addPlayerSelf player = this.game.Value |> Interaction.addPlayerSelf player

//    let addPlayersSelf players =
//        this.game.Value |> Interaction.interact (fun g -> players |> List.map (fun player -> g.AddPlayer { RequestingPlayer = player; PlayerToAdd = player }))

//    let addPlayerIsAcknowledged response =
//        match response with
//        | AddPlayerResponse.AddPlayerAcknowledged _ -> true
//        | AddPlayerResponse.AddPlayerRejected _ -> false

//    let addPlayerSelfAcknowledged player =
//        let response, _ = this.addPlayerSelf player
//        match response with
//        | AddPlayerResponse.AddPlayerAcknowledged a -> a.Acknowledgment
//        | AddPlayerResponse.AddPlayerRejected r -> raise (Exception r.Rejection)

//    let removePlayerSelf player = this.game.Value |> Interaction.removePlayerSelf player

//    let removePlayersSelf players =
//        this.game.Value |> Interaction.interact (fun g -> players |> List.map (fun player -> g.RemovePlayer { RequestingPlayer = player; PlayerToRemove = player }))

//    let removePlayerSelfIsAcknowledged response =
//        match response with
//        | RemovePlayerResponse.RemovePlayerAcknowledged _ -> true
//        | RemovePlayerResponse.RemovePlayerRejected _ -> false

//    let removePlayerAcknowledged player =
//        let response, _ = this.removePlayerSelf player
//        match response with
//        | RemovePlayerResponse.RemovePlayerAcknowledged a -> a.Acknowledgment
//        | RemovePlayerResponse.RemovePlayerRejected r -> raise (Exception r.Rejection)

//    [<SetUp>]
//    let SetUp () =
//        this.game.Value' <- Some (game.Value
//            (
//                this.channelMessages.Publish |> Observable.asObservable,
//                this.playerName,
//                this.isMod,
//                this.minimumPlayers,
//                this.reminderTimeSpan,
//                this.autoSkipTimeSpan
//            ) :> Igame.Value)
