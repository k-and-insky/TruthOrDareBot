namespace TruthOrDareBot

open System
open System.Threading.Tasks
open System.Linq
open Discord
open Discord.Commands

type IClient =
    inherit IDisposable
    abstract member ExecuteSynchronously : unit -> unit

type Client(token : string, serverName : string, channelName : string) =
    let game = new TruthOrDareBot.Game() :> IGame

    let (|?) lhs rhs = (if lhs = null then rhs else lhs)

    let userName (channel : Channel) (player : Player) =
        let user = channel.GetUser player
        user.Nickname |? user.Name

    let client = (new DiscordClient()).UsingCommands (fun c ->
        c.AllowMentionPrefix <- true
        c.PrefixChar <- Nullable '$'
        c.HelpMode <- HelpMode.Public
    )

    let commandService = client.GetService<CommandService>()

    do commandService
        .CreateCommand("add me")
        .Description("Adds you to the queue")
        .Do (fun commandEvent ->
            let channel = commandEvent.Channel
            let requestingUser = commandEvent.User

            let mentionPlayer player =
                let user = channel.GetUser player
                user.NicknameMention

            let response = game.AddPlayer { Player = requestingUser.Id }

            let reply = match response.AddPlayerStatus with
                        | AddPlayerAcknowledged { Player = addedPlayer; Acknowledgment = acknowledgment } ->
                            let mention = mentionPlayer addedPlayer
                            sprintf "%s: %s" mention acknowledgment
                        | AddPlayerRejected { Player = rejectedPlayer; Rejection = rejection } ->
                            let mention = mentionPlayer rejectedPlayer
                            sprintf "%s: %s" mention rejection

            async {
                let! message = channel.SendMessage reply |> Async.AwaitTask
                ()
            } |> Async.Start

            match response.AddPlayerStatus with
            | AddPlayerAcknowledged { GameStatus = gameStatus } ->
                let gameStatusAcknowledgment = gameStatus.StartStatus.Acknowledgment
                async {
                    let! message = channel.SendMessage gameStatusAcknowledgment |> Async.AwaitTask
                    ()
                } |> Async.Start
            | _ -> ()
        )

    do commandService
        .CreateCommand("remove me")
        .Description("Removes you from the queue")
        .Do (fun commandEvent ->
            let channel = commandEvent.Channel
            let requestingUser = commandEvent.User

            let mentionPlayer player =
                let user = channel.GetUser player
                user.NicknameMention

            let response = game.RemovePlayer { Player = requestingUser.Id }

            let reply = match response.RemovePlayerStatus with
                        | RemovePlayerAcknowledged { Player = removedPlayer; Acknowledgment = acknowledgment } ->
                            let mention = mentionPlayer removedPlayer
                            sprintf "%s: %s" mention acknowledgment
                        | RemovePlayerRejected { Player = rejectedPlayer; Rejection = rejection } ->
                            let mention = mentionPlayer rejectedPlayer
                            sprintf "%s: %s" mention rejection

            async {
                let! message = channel.SendMessage reply |> Async.AwaitTask
                ()
            } |> Async.Start

            match response.RemovePlayerStatus with
            | RemovePlayerAcknowledged { GameStatus = gameStatus } ->
                let gameStatusAcknowledgment = gameStatus.StartStatus.Acknowledgment
                async {
                    let! message = channel.SendMessage gameStatusAcknowledgment |> Async.AwaitTask
                    ()
                } |> Async.Start
            | _ -> ()
        )

    do commandService
        .CreateCommand("show queue")
        .Alias("show q")
        .Description("Shows the queue")
        .Do (fun commandEvent ->
            let channel = commandEvent.Channel
            let requestingUser = commandEvent.User

            let response = game.ShowQueue { Player = requestingUser.Id }

            let reply = match response.ShowQueueStatus with
                        | ShowQueueAcknowledged { Queue = queue } ->
                            let queueText = queue |> Seq.map (fun p -> userName channel p) |> String.concat "\n"
                            "Current queue:\n=============\n" + queueText
                        | ShowQueueRejected { Rejection = rejection } ->
                            rejection

            async {
                let! message = channel.SendMessage reply |> Async.AwaitTask
                ()
            } |> Async.Start
        )

    do commandService
        .CreateCommand("whose turn")
        .Alias("whose turn is it", "who's up", "whos up")
        .Description("Shows the current player")
        .Do (fun commandEvent ->
            let channel = commandEvent.Channel
            let requestingUser = commandEvent.User

            let response = game.WhoseTurn { Player = requestingUser.Id }

            let reply = match response.WhoseTurnStatus with
                        | WhoseTurnAcknowledged { CurrentAsker = currentAsker; CurrentPlayer = currentPlayer } ->
                            sprintf "%s is asking %s" (userName channel currentAsker) (userName channel currentPlayer)
                        | WhoseTurnRejected { Rejection = rejection } ->
                            rejection

            async {
                let! message = channel.SendMessage reply |> Async.AwaitTask
                ()
            } |> Async.Start
        )

    do commandService
        .CreateCommand("love me")
        .Do (fun commandEvent ->
            let channel = commandEvent.Channel
            let user = commandEvent.User

            async {
                let response = sprintf "%s: *pat pat*" user.NicknameMention
                let! message = channel.SendMessage response |> Async.AwaitTask
                ()
            } |> Async.Start
        )

    do commandService
        .CreateCommand("i love you")
        .Do (fun commandEvent ->
            let channel = commandEvent.Channel
            let user = commandEvent.User

            async {
                let response = sprintf "%s: I love you too" user.NicknameMention
                let! message = channel.SendMessage response |> Async.AwaitTask
                ()
            } |> Async.Start
        )

    interface IDisposable with
        member this.Dispose() =
            client.Dispose()

    interface IClient with
        member this.ExecuteSynchronously() =
            client.ExecuteAndWait (Func<Task> (fun () ->
                async {
                    try
                        let! _ = client.Connect(token, TokenType.Bot) |> Async.AwaitTask
                        printfn "Connected"
                    with error ->
                        Console.WriteLine "Failed to connect"
                        raise error

                    client.ServerAvailable
                        |> Event.filter (fun args -> args.Server.Name = serverName)
                        |> Event.add (fun args ->
                            let server = args.Server
                            printfn "Joined server %s" server.Name

                            let channel =
                                try
                                    let allChannels = server.AllChannels |> Seq.map (fun c -> c.Name) |> String.concat ", "
                                    printfn "All channels: %s" allChannels
                                    let channel = server.FindChannels(channelName, ChannelType.Text, true).Single()
                                    printfn "Found channel %s" channel.Name
                                    channel
                                with error ->
                                    Console.WriteLine "Failed to find channel"
                                    raise error
                            
                            async {
                                let! message = channel.SendMessage "Bot started" |> Async.AwaitTask
                                ()
                            } |> Async.Start
                        )

                    ()
                } |> Async.StartAsTask :> Task
            ))