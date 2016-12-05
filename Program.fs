open System
open TruthOrDareBot

let token = "MjU0NDU0ODQ3MzA1Mjg1NjMz.CyPTHw.bTlyjOc5_pMY7InHkAOCqt7-gJg"
let serverName = "truth-or-dare-bot-test"
let channelName = "truth-or-dare"

[<EntryPoint>]
let main _ = 
    printfn "Starting TruthOrDareBot"
    use client = new Client(token, serverName, channelName) :> IClient
    do client.ExecuteSynchronously()
    0