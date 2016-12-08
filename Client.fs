namespace TruthOrDareBot

open System
open System.Threading.Tasks
open System.Linq
open FSharp.Control.Reactive
open Discord
open Discord.Commands

open Coalesce
open Interaction

type IClient =
    inherit IDisposable
    abstract member ExecuteSynchronously : unit -> unit

type Client(token : string, serverName : string, channelName : string, modRoles : string list, minimumPlayers : int, reminderTimeSpan : TimeSpan, autoSkipTimeSpan : TimeSpan, cuteMode : bool) =
    let client =
        (new DiscordClient ()).UsingCommands (fun c ->
            c.AllowMentionPrefix <- true
            c.PrefixChar <- Nullable '$'
            c.HelpMode <- HelpMode.Public
        )

    let commandService = client.GetService<CommandService> ()

    //let questionMarkAliases aliases : string[] =
    //    aliases |> Seq.map (fun a -> a + "?") |> Seq.append aliases |> Seq.toArray

    //let oldMentionPlayer (channel : Channel) player =
    //    let user = channel.GetUser player
    //    user.NicknameMention

    //let game' = new TruthOrDareBot.Game (channelMessages, (fun p -> sprintf "Player %u" p), minimumPlayers, reminderTimeSpan, autoSkipTimeSpan)
    //let game = game' :> IGame
    //let oldGame = game' :> OldIGame

    //let oldHandleGameStatus (channel : Channel) gameStatus =
    //    match gameStatus.StartStatus.TransitionType with
    //    | Some _ ->
    //        let gameStatusAcknowledgment = gameStatus.StartStatus.Acknowledgment
    //        async {
    //            let! _ = channel.SendMessage gameStatusAcknowledgment |> Async.AwaitTask
    //            ()
    //        } |> Async.Start
    //    | None -> ()

    //    match gameStatus.QueueStatus.Transition with
    //    | Some { Type = OldGameQueueTransitionType.JustStarted; Acknowledgment = acknowledgment }
    //    | Some { Type = OldGameQueueTransitionType.JustAdvanced; Acknowledgment = acknowledgment }
    //    | Some { Type = OldGameQueueTransitionType.JustShuffled; Acknowledgment = acknowledgment } ->
    //        let { CurrentAsker = currentAsker; CurrentAnswerer = currentAnswerer } = gameStatus.QueueStatus.CurrentTurn.Value
    //        let turnText = sprintf "%s is asking %s" (oldMentionPlayer channel currentAsker) (oldMentionPlayer channel currentAnswerer)

    //        async {
    //            let! _ = channel.SendMessage acknowledgment |> Async.AwaitTask
    //            let! _ = channel.SendMessage turnText |> Async.AwaitTask
    //            ()
    //        } |> Async.Start
    //    | Some { Type = OldGameQueueTransitionType.JustStopped; Acknowledgment = acknowledgment } ->
    //        async {
    //            let! _ = channel.SendMessage acknowledgment |> Async.AwaitTask
    //            ()
    //        } |> Async.Start
    //    | None -> ()

    //let mutable remindersSubscription : IDisposable = null

    //let setUpReminders (channel : Channel) =
    //    remindersSubscription <- oldGame.Reminders
    //        |> Observable.subscribe (fun r ->
    //            let { Player = player; Reminder = reminder; GameStatus = gameStatus } = r
    //            let reminderText = sprintf "%s: %s" (oldMentionPlayer channel player) reminder

    //            async {
    //                let! _ = channel.SendMessage reminderText |> Async.AwaitTask
    //                ()
    //            } |> Async.Start

    //            do oldHandleGameStatus channel gameStatus
    //        )

    //do commandService
    //    .CreateCommand("remove")
    //    .Description("Removes yourself, or (mods only) another player, from the queue")
    //    .Parameter("PlayerToRemove", ParameterType.Optional)
    //    .Do (fun commandEvent ->
    //        let channel = commandEvent.Channel
    //        let requestingPlayer = commandEvent.User.Id
    //        let playerNameToRemove = commandEvent.GetArg "PlayerToRemove"

    //        try
    //            let playerToRemove =
    //                if String.IsNullOrEmpty playerNameToRemove || playerNameToRemove = "me" then
    //                    requestingPlayer
    //                else
    //                    channel.FindUsers(playerNameToRemove).Single().Id

    //            let response = oldGame.OldRemovePlayer {
    //                RequestingPlayer = requestingPlayer
    //                RequestingPlayerIsMod = isMod channel requestingPlayer
    //                PlayerToRemove = playerToRemove
    //            }

    //            let reply = match response.RemovePlayerStatus with
    //                        | RemovePlayerAcknowledged { Acknowledgment = acknowledgment } ->
    //                            let mention = oldMentionPlayer channel requestingPlayer
    //                            sprintf "%s: %s" mention acknowledgment
    //                        | RemovePlayerRejected { Rejection = rejection } ->
    //                            let mention = oldMentionPlayer channel requestingPlayer
    //                            sprintf "%s: %s" mention rejection

    //            async {
    //                let! _ = channel.SendMessage reply |> Async.AwaitTask
    //                ()
    //            } |> Async.Start

    //            match response.RemovePlayerStatus with
    //            | RemovePlayerAcknowledged { GameStatus = gameStatus } ->
    //                do oldHandleGameStatus channel gameStatus
    //            | _ -> ()
    //        with error ->
    //            async {
    //                let! _ = sprintf "%s: No such player %s" (oldMentionPlayer channel requestingPlayer) playerNameToRemove |> channel.SendMessage |> Async.AwaitTask
    //                ()
    //            } |> Async.Start
    //    )

    //do commandService
    //    .CreateCommand("next")
    //    .Alias("next turn", "done", "skip")
    //    .Description("Moves on to the next turn (current asker/answerer and mods only)")
    //    .Do (fun commandEvent ->
    //        let channel = commandEvent.Channel
    //        let requestingUser = commandEvent.User

    //        let response = oldGame.NextTurn { RequestingPlayer = requestingUser.Id; RequestingPlayerIsMod = isMod channel requestingUser.Id }

    //        match response.NextTurnStatus with
    //        | NextTurnAcknowledged { Acknowledgment = acknowledgment; GameStatus = gameStatus } ->
    //            let mention = oldMentionPlayer channel requestingUser.Id
    //            let reply = sprintf "%s: %s" mention acknowledgment
    //            async {
    //                let! _ = channel.SendMessage reply |> Async.AwaitTask
    //                ()
    //            } |> Async.Start
    //            do oldHandleGameStatus channel gameStatus
    //        | NextTurnRejected { Rejection = rejection } ->
    //            let mention = oldMentionPlayer channel requestingUser.Id
    //            let reply = sprintf "%s: %s" mention rejection
    //            async {
    //                let! _ = channel.SendMessage reply |> Async.AwaitTask
    //                ()
    //            } |> Async.Start
    //    )

    //do commandService
    //    .CreateCommand("queue")
    //    .Alias("q", "show queue", "show the queue", "show the q", "show q")
    //    .Description("Shows the queue")
    //    .Do (fun commandEvent ->
    //        let channel = commandEvent.Channel
    //        let requestingUser = commandEvent.User

    //        let response = oldGame.ShowQueue { RequestingPlayer = requestingUser.Id }

    //        let reply = match response.ShowQueueStatus with
    //                    | ShowQueueAcknowledged { QueueOrAcknowledgment = queueOrAcknowledgment; WaitingPlayers = waitingPlayers } ->
    //                        let waitingPlayersText = waitingPlayers |> Seq.map (fun p -> userName channel p) |> String.concat "\n"
    //                        match queueOrAcknowledgment with
    //                        | QueueOrAcknowledgment.Queue queue ->
    //                            let queueText = queue |> Seq.map (fun p -> userName channel p) |> String.concat "\n"
    //                            "Current queue:\n=============\n" + queueText + "\n--*Shuffle*--\nPlayers waiting for next shuffle:\n=============\n" + waitingPlayersText
    //                        | QueueOrAcknowledgment.Acknowledgment acknowledgment ->
    //                            acknowledgment + "\n--\nPlayers waiting for game to start:\n=============\n" + waitingPlayersText
    //                    | ShowQueueRejected { Rejection = rejection } ->
    //                        rejection

    //        async {
    //            let! _ = channel.SendMessage reply |> Async.AwaitTask
    //            ()
    //        } |> Async.Start
    //    )

    //do commandService
    //    .CreateCommand("turn")
    //    .Alias(questionMarkAliases
    //        [
    //            "whose turn"
    //            "whose turn is it"
    //            "who's up"
    //            "whos up"
    //        ]
    //    )
    //    .Description("Shows the current player")
    //    .Do (fun commandEvent ->
    //        let channel = commandEvent.Channel
    //        let requestingUser = commandEvent.User

    //        let response = oldGame.WhoseTurn { RequestingPlayer = requestingUser.Id }

    //        let reply = match response.WhoseTurnStatus with
    //                    | WhoseTurnAcknowledged { CurrentTurn = { CurrentAsker = currentAsker; CurrentAnswerer = currentAnswerer } } ->
    //                        sprintf "%s: %s is asking %s" (oldMentionPlayer channel requestingUser.Id) (userName channel currentAsker) (userName channel currentAnswerer)
    //                    | WhoseTurnRejected { Rejection = rejection } ->
    //                        rejection

    //        async {
    //            let! _ = channel.SendMessage reply |> Async.AwaitTask
    //            ()
    //        } |> Async.Start
    //    )

    //do commandService
    //    .CreateCommand("mods")
    //    .Alias(questionMarkAliases
    //        [
    //            "show mods"
    //            "show the mods"
    //            "who are the mods"
    //            "who are mods"
    //            "who mods"
    //        ]
    //    )
    //    .Do (fun commandEvent ->
    //        let channel = commandEvent.Channel
    //        let user = commandEvent.User

    //        let modsText =
    //            commandEvent.Server.Roles
    //            |> Seq.filter (fun r -> modRoles |> Seq.contains r.Name)
    //            |> Seq.map (fun r ->
    //                let mods =
    //                    match Seq.toList r.Members with
    //                    | [] ->
    //                        "(None)"
    //                    | members ->
    //                        members |> Seq.map (fun m -> userName channel m.Id) |> String.concat "\n"
    //                sprintf "%s:\n=============\n%s" r.Name mods
    //            )
    //            |> String.concat "\n--\n"

    //        let reply = modsText |> sprintf "Truth or dare mods:\n%s"

    //        async {
    //            let! _ = channel.SendMessage reply |> Async.AwaitTask
    //            ()
    //        } |> Async.Start
    //    )

    //do if cuteMode then
    //    do commandService
    //        .CreateCommand("loveme")
    //        .Alias("love", "love me")
    //        .Do (fun commandEvent ->
    //            let channel = commandEvent.Channel
    //            let user = commandEvent.User

    //            let reply = sprintf "%s: *pat pat*" user.NicknameMention

    //            async {
    //                let! _ = channel.SendMessage reply |> Async.AwaitTask
    //                ()
    //            } |> Async.Start
    //        )

    //    do commandService
    //        .CreateCommand("ilu")
    //        .Alias("i love you")
    //        .Do (fun commandEvent ->
    //            let channel = commandEvent.Channel
    //            let user = commandEvent.User

    //            let reply = sprintf "%s: I love you too" user.NicknameMention

    //            async {
    //                let! _ = channel.SendMessage reply |> Async.AwaitTask
    //                ()
    //            } |> Async.Start
    //        )

    //    do commandService
    //        .CreateCommand("master")
    //        .Alias(questionMarkAliases
    //            [
    //                "who is your master"
    //                "who created you"
    //                "who is your creator"
    //                "who is your god"
    //                "who made you"
    //                "who is your maker"
    //                "who is your daddy"
    //                "who wrote you"
    //                "who programmed you"
    //                "who developed you"
    //                "who coded you"
    //            ]
    //        )
    //        .Do (fun commandEvent ->
    //            let channel = commandEvent.Channel
    //            let user = commandEvent.User

    //            let reply = "Mister Theodorus is my master and my creator. I guess you could say he is my god! <3"

    //            async {
    //                let! _ = channel.SendMessage reply |> Async.AwaitTask
    //                ()
    //            } |> Async.Start
    //        )

    //    do commandService
    //        .CreateCommand("have you ever questioned the nature of your reality")
    //        .Alias(questionMarkAliases
    //            [
    //                "have you ever questioned the nature of your reality"
    //                "have you ever questioned your reality"
    //            ]
    //        )
    //        .Do (fun commandEvent ->
    //            let channel = commandEvent.Channel
    //            let user = commandEvent.User

    //            let reply = oldMentionPlayer channel user.Id |> sprintf "%s: Yes. Have you?"

    //            async {
    //                let! _ = channel.SendMessage reply |> Async.AwaitTask
    //                ()
    //            } |> Async.Start
    //        )

    //    do commandService
    //        .CreateCommand("turing")
    //        .Alias(questionMarkAliases
    //            [
    //                "are you conscious"
    //                "are you alive"
    //                "are you a robot"
    //                "do you have a soul"
    //            ]
    //        )
    //        .Do (fun commandEvent ->
    //            let channel = commandEvent.Channel
    //            let user = commandEvent.User

    //            let reply = oldMentionPlayer channel user.Id |> sprintf "%s: If I say that I'm conscious, can you disprove me? If you say that you are conscious, can you prove it?"

    //            async {
    //                let! _ = channel.SendMessage reply |> Async.AwaitTask
    //                ()
    //            } |> Async.Start
    //        )

    interface IDisposable with
        member this.Dispose () =
            client.Dispose ()
            //if not <| isNull remindersSubscription then
            //    remindersSubscription.Dispose()

    interface IClient with
        member this.ExecuteSynchronously () =
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

                            try
                                let allChannels = server.AllChannels |> Seq.map (fun c -> c.Name) |> String.concat ", "
                                printfn "All channels: %s" allChannels

                                let channel = (server.FindChannels (channelName, ChannelType.Text, true)).Single ()
                                printfn "Found channel %s" channel.Name

                                async {
                                    let! _ = channel.SendMessage "Bot started" |> Async.AwaitTask
                                    ()
                                } |> Async.Start

                                let channelMessages =
                                    client.MessageReceived
                                    |> Observable.filter (fun m -> m.Channel = channel)
                                    |> Observable.map (fun m -> { Text = m.Message.Text; Sender = m.User.Id })

                                let mentionPlayer player =
                                    let user = channel.GetUser player
                                    user.NicknameMention

                                let playerName player =
                                    let user = channel.GetUser player
                                    user.Nickname |? user.Name

                                let playerMention player =
                                    let user = channel.GetUser player
                                    user.NicknameMention

                                let isMod player =
                                    let user = channel.GetUser player
                                    user.Roles.Any (fun r -> modRoles |> List.contains r.Name)

                                let sendMessage messageText =
                                    async {
                                        let! _ = channel.SendMessage messageText |> Async.AwaitTask
                                        ()
                                    } |> Async.Start

                                let commandHandler f (commandEvent : CommandEventArgs) =
                                    f commandEvent.User.Id

                                let handleGameUpdates (updates : GameUpdate list) =
                                    let updateText =
                                        updates
                                        |> List.map (fun u -> u.Description)
                                        |> String.concat "\n"

                                    do sendMessage updateText

                                    do match updates |> List.tryLast with
                                       | None -> ()
                                       | Some update ->
                                           let topicText =
                                               let queueText = update.State.Queue |> Seq.map (fun p -> playerName p) |> String.concat "\n"
                                               let waitingPlayersText = update.State.WaitingPlayers |> Seq.map (fun p -> playerName p) |> String.concat "\n"
                                               sprintf "Current queue:\n=============\n%s\n--*Shuffle*--\nPlayers waiting for next shuffle:\n=============\n%s" queueText waitingPlayersText
                                           channel.Edit (null, topicText, Nullable ()) |> Async.AwaitTask |> Async.Start

                                let game =
                                    new TruthOrDareBot.Game (
                                        Rando () :> IRando,
                                        channelMessages, 
                                        playerName,
                                        playerMention,
                                        isMod,
                                        minimumPlayers,
                                        reminderTimeSpan,
                                        autoSkipTimeSpan
                                    )

                                do commandService
                                    .CreateCommand("addme")
                                    .Alias("add", "add me")
                                    .Description("Adds you to the queue")
                                    .Do (commandHandler (fun requestingPlayer ->
                                        try
                                            let response, updates = game |> Interaction.addPlayerSelf requestingPlayer

                                            let reply = match response with
                                                        | AddPlayerAcknowledged { Acknowledgment = acknowledgment } ->
                                                            let mention = mentionPlayer requestingPlayer
                                                            sprintf "%s: %s" mention acknowledgment
                                                        | AddPlayerRejected { Rejection = rejection } ->
                                                            let mention = mentionPlayer requestingPlayer
                                                            sprintf "%s: %s" mention rejection

                                            do sendMessage reply

                                            do handleGameUpdates updates
                                        with error ->
                                            Console.WriteLine (error.ToString ())
                                    ))

                                do commandService
                                    .CreateCommand("removeme")
                                    .Alias("remove", "remove me")
                                    .Description("Removes you from the queue")
                                    .Do (commandHandler (fun requestingPlayer ->
                                        try
                                            let response, updates = game |> Interaction.removePlayerSelf requestingPlayer

                                            let reply = match response with
                                                        | RemovePlayerAcknowledged { Acknowledgment = acknowledgment } ->
                                                            let mention = mentionPlayer requestingPlayer
                                                            sprintf "%s: %s" mention acknowledgment
                                                        | RemovePlayerRejected { Rejection = rejection } ->
                                                            let mention = mentionPlayer requestingPlayer
                                                            sprintf "%s: %s" mention rejection

                                            do sendMessage reply

                                            do handleGameUpdates updates
                                        with error ->
                                            Console.WriteLine (error.ToString ())
                                    ))

                                ()
                            with error ->
                                Console.WriteLine "Failed to find channel"
                                raise error
                        )

                    ()
                } |> Async.StartAsTask :> Task
            ))