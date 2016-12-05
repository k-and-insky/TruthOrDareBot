open System
open TruthOrDareBot

let token = "MjU0NDU0ODQ3MzA1Mjg1NjMz.CyPTHw.bTlyjOc5_pMY7InHkAOCqt7-gJg"
let serverName = "truth-or-dare-bot-test"
let channelName = "truth-or-dare"
let modRoles = ["Administrators"; "Moderators"; "Staff"; "Ambassadors"]
let minimumPlayers = 4
let reminderTimeSpan = TimeSpan(0, 3, 0)
let autoSkipTimeSpan = TimeSpan(0, 5, 0)

[<EntryPoint>]
let main _ = 
    printfn "Starting TruthOrDareBot"
    use client = new Client(token, serverName, channelName, modRoles, minimumPlayers, reminderTimeSpan, autoSkipTimeSpan) :> IClient
    do client.ExecuteSynchronously()
    0