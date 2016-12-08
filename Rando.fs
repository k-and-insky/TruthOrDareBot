namespace TruthOrDareBot

open System

type IRando =
    abstract member Sample : 'a list -> 'a
    abstract member Shuffle : 'a list -> 'a list

type Rando () =
    let random = Random ()

    interface IRando with
        member this.Sample (ls : 'a list) =
            ls.[random.Next ls.Length]

        member this.Shuffle ls =
            ls |> List.sortBy (fun e -> random.Next())