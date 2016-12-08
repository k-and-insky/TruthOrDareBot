namespace TruthOrDareBot

open System

module Interaction =
    let interact (f : IGame -> 'a) (game : IGame) =
        let mutable updates : GameUpdate list = []
        use subscription = game.Updates |> Observable.subscribe (fun u ->
                updates <- List.append updates [u]
            )
        let result = f game
        result, updates

    let addPlayerSelf player =
        interact <| fun game -> game.AddPlayer { RequestingPlayer = player; PlayerToAdd = player }

    let addPlayerOther requestingPlayer playerToAdd =
        interact <| fun game -> game.AddPlayer { RequestingPlayer = requestingPlayer; PlayerToAdd = playerToAdd }

    let removePlayerSelf player =
        interact <| fun game -> game.RemovePlayer { RequestingPlayer = player; PlayerToRemove = player }

    let removePlayerOther requestingPlayer playerToRemove =
        interact <| fun game -> game.RemovePlayer { RequestingPlayer = requestingPlayer; PlayerToRemove = playerToRemove }