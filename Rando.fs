namespace TruthOrDareBot

open System

module Rando =
    let random = Random()

    let sample (ls : 'a list) =
        ls.[random.Next ls.Length]

    let shuffle ls =
        ls |> List.sortBy (fun e -> random.Next())