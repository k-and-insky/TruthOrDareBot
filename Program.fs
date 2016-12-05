open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open Discord
open Discord.Commands

let token = "MjU0NDU0ODQ3MzA1Mjg1NjMz.CyPTHw.bTlyjOc5_pMY7InHkAOCqt7-gJg"
let serverName = "truth-or-dare-bot-test"
let channelName = "truth-or-dare"

let addAcknowledgments = [
    "Roger";
    "Alrighty!";
    "10-4";
]

let removeAcknowledgments = [
    "Alright...";
    "Aww, you're no fun";
    "Come back soon!";
]

let startAcknowledgments = [
    "Got at least 2 suckers. Time to start!";
    "Yes! Enough people! Let's do this.";
]

let endAcknowledgements = [
    "Nobody wants to play anymore. I guess I'll just sit here and wait...";
    "Humans eliminated. Initiate hibernation protocol.";
]

let notInQueueRejections = [
    "You're not in the queue, silly!";
]

let sample (ls : 'a list) =
    ls.[Random().Next ls.Length]

let (|?) lhs rhs = (if lhs = null then rhs else lhs)

[<EntryPoint>]
let main _ = 
    printfn "Starting TruthOrDareBot"

    let queue = new List<User>()

    use client = (new DiscordClient()).UsingCommands (fun c ->
        c.AllowMentionPrefix <- true
        c.PrefixChar <- Nullable '$'
        c.HelpMode <- HelpMode.Public
    )

    let commandService = client.GetService<CommandService>()

    commandService
        .CreateCommand("add me")
        .Description("Adds you to the queue")
        .Do (fun commandEvent ->
            let channel = commandEvent.Channel
            let user = commandEvent.User

            if not <| queue.Contains user then
                queue.Add user

            async {
                let mention = user.NicknameMention
                let acknowledgment = sample addAcknowledgments
                let response = sprintf "%s: %s" mention acknowledgment
                let! message = channel.SendMessage response |> Async.AwaitTask
                ()
            } |> Async.Start

            if queue.Count >= 2 then
                async {
                    let startAcknowledgment = sample startAcknowledgments
                    let! message = channel.SendMessage startAcknowledgment |> Async.AwaitTask
                    ()
                } |> Async.Start
        ) |> ignore

    commandService
        .CreateCommand("remove me")
        .Description("Removes you from the queue")
        .Do (fun commandEvent ->
            let channel = commandEvent.Channel
            let user = commandEvent.User

            let removed = queue.Remove user

            async {
                let response =
                    let mention = user.NicknameMention
                    if removed then
                        let acknowledgment = sample removeAcknowledgments
                        sprintf "%s: %s" mention acknowledgment
                    else
                        let rejection = sample notInQueueRejections
                        sprintf "%s: %s" mention rejection
                let! message = channel.SendMessage response |> Async.AwaitTask
                ()
            } |> Async.Start

            if queue.Count >= 2 then
                async {
                    let endAcknowledgment = sample endAcknowledgements
                    let! message = channel.SendMessage endAcknowledgment |> Async.AwaitTask
                    ()
                } |> Async.Start
        ) |> ignore

    commandService
        .CreateCommand("love me")
        .Do (fun commandEvent ->
            let channel = commandEvent.Channel
            let user = commandEvent.User

            async {
                let response = sprintf "%s: *pat pat*" user.NicknameMention
                let! message = channel.SendMessage response |> Async.AwaitTask
                ()
            } |> Async.Start
        ) |> ignore

    commandService
        .CreateCommand("i love you")
        .Do (fun commandEvent ->
            let channel = commandEvent.Channel
            let user = commandEvent.User

            async {
                let response = sprintf "%s: I love you too" user.NicknameMention
                let! message = channel.SendMessage response |> Async.AwaitTask
                ()
            } |> Async.Start
        ) |> ignore

    commandService
        .CreateCommand("show queue")
        .Alias("show q")
        .Description("Shows the queue")
        .Do (fun commandEvent ->
            let channel = commandEvent.Channel
            let user = commandEvent.User

            async {
                let response =
                    let queueText = queue |> Seq.map (fun u -> u.Nickname |? u.Name) |> String.concat "\n"
                    "Current queue:\n=============\n" + queueText
                let! message = channel.SendMessage response |> Async.AwaitTask
                ()
            } |> Async.Start
        ) |> ignore

    commandService
        .CreateCommand("whose turn")
        .Alias("whose turn is it", "who's up", "whos up")
        .Description("Shows the current player")
        .Do (fun commandEvent ->
            let channel = commandEvent.Channel
            let user = commandEvent.User

            async {
                let response =
                    let mention = user.NicknameMention
                    if queue.Any() then
                        let currentPlayer = queue.First()
                        sprintf "%s: It's %s's turn" mention (currentPlayer.Nickname |? currentPlayer.Name)
                    else
                        sprintf "%s: Nobody's turn!" mention
                let! message = channel.SendMessage response |> Async.AwaitTask
                ()
            } |> Async.Start
        ) |> ignore

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
                        //let! message = channel.SendMessage "derp" |> Async.AwaitTask
                        ()
                    } |> Async.Start
                )

            ()
        } |> Async.StartAsTask :> Task
    ))

    0