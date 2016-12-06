open System
open TruthOrDareBot

open Coalesce

let token = Environment.GetEnvironmentVariable "TRUTH_OR_DARE_BOT_TOKEN"
let serverName = Environment.GetEnvironmentVariable "TRUTH_OR_DARE_BOT_SERVER" |? "truth-or-dare-bot-test"
let channelName = Environment.GetEnvironmentVariable "TRUTH_OR_DARE_BOT_CHANNEL" |? "truth-or-dare"
let modRoles = (Environment.GetEnvironmentVariable "TRUTH_OR_DARE_BOT_MOD_ROLES" |? "Administrators; Moderators; Staff; Ambassadors").Split ';' |> Seq.map (fun s -> s.Trim()) |> Seq.toList
let minimumPlayers = Environment.GetEnvironmentVariable "TRUTH_OR_DARE_BOT_MINIMUM_PLAYERS" |? "4" |> Int32.Parse
let reminderTimeSpan = TimeSpan(0, Environment.GetEnvironmentVariable "TRUTH_OR_DARE_BOT_REMINDER_MINUTES" |? "3" |> Int32.Parse, 0)
let autoSkipTimeSpan = TimeSpan(0, Environment.GetEnvironmentVariable "TRUTH_OR_DARE_BOT_AUTO_SKIP_MINUTES" |? "5" |> Int32.Parse, 0)
let cuteMode = Environment.GetEnvironmentVariable "TRUTH_OR_DARE_CUTE_MODE" |? "false" |> Boolean.Parse

[<EntryPoint>]
let main _ = 
    printfn "Starting TruthOrDareBot"
    use client = new Client(token, serverName, channelName, modRoles, minimumPlayers, reminderTimeSpan, autoSkipTimeSpan, cuteMode) :> IClient
    do client.ExecuteSynchronously()
    0