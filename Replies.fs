namespace TruthOrDareBot

open System

module Replies =
    let addPlayerAcknowledgments =
        [
            "Roger that."
            "Alrighty!"
            "10-4, good buddy."
        ]

    let addPlayerAcknowledged (rando : IRando) =
        rando.Sample addPlayerAcknowledgments

    let addPlayerRejections =
        [
            "You're already in the game, dingus."
        ]

    let addPlayerRejected (rando : IRando) =
        rando.Sample addPlayerRejections

    let playerAddedWaitingUpdateDescriptions playerName playersRemaining =
        let descriptions : Printf.StringFormat<string -> int -> string> list = [
            "Added %s to waiting list. Waiting for %i more players..."
        ]
        descriptions |> List.map (fun s -> sprintf s playerName playersRemaining)

    let playerAddedWaitingUpdateDescription playerName playersRemaining (rando : IRando) =
        playerAddedWaitingUpdateDescriptions playerName playersRemaining |> rando.Sample

    let playerAddedToActiveQueueUpdateDescriptions playerName =
        let descriptions : Printf.StringFormat<string -> string> list = [
            "Added %s to active queue."
        ]
        descriptions |> List.map (fun s -> sprintf s playerName)

    let playerAddedToActiveQueueUpdateDescription playerName (rando : IRando) =
        playerAddedToActiveQueueUpdateDescriptions playerName |> rando.Sample

    let removePlayerSelfAcknowledgments =
        [
            "Aww, you're no fun."
            "Alright... Leave if you must."
        ]

    let removePlayerSelfAcknowledged (rando : IRando) =
        rando.Sample removePlayerSelfAcknowledgments

    let removePlayerOtherAcknowledgments =
        [
            "Alright, I removed them, but I'm not happy about it."
            "For you, anything. *Poof!* They're gone."
        ]

    let removePlayerOtherAcknowledged (rando : IRando) =
        rando.Sample removePlayerOtherAcknowledgments

    let removePlayerSelfNotInGameRejections =
        [
            "You're not in the game, silly!"
        ]

    let removePlayerSelfNotInGameRejected (rando : IRando) =
        rando.Sample removePlayerSelfNotInGameRejections

    let removePlayerOtherNotInGameRejected (rando : IRando) =
        rando.Sample [
            "Um, they're not in the game..."
        ]

    let removePlayerOtherNotModRejected (rando : IRando) =
        rando.Sample [
            "Only mods can remove other players from the game."
        ]

    let stayedStoppedGameAcknowledged (rando : IRando) =
        rando.Sample [
            "Still waiting for more players..."
        ]
    
    let stayedInProgressGameAcknowledged (rando : IRando) =
        rando.Sample [
            "Game in progress."
        ]

    let justStartedGameAcknowledgments =
        [
            "Go time."
            "Alright chums, let's do this."
        ]

    let justStartedGameAcknowledged (rando : IRando) =
        rando.Sample justStartedGameAcknowledgments

    let justStoppedGameAcknowledgments =
        [
            "Boo, the game is stopped. :("
        ]

    let justStoppedGameAcknowledged (rando : IRando) =
        rando.Sample justStartedGameAcknowledgments

    let waitingForGameStartAcknowledgments =
        [
            "Waiting for game to start."
        ]

    let waitingForGameStartAcknowledged (rando : IRando) =
        rando.Sample waitingForGameStartAcknowledgments

    let showQueueGameStoppedAcknowledgments =
        [
            "There is no queue to show!"
            "No game in progress."
            "The game is stopped."
        ]

    let showQueueGameStoppedAcknowledged (rando : IRando) =
        rando.Sample showQueueGameStoppedAcknowledgments

    let whoseTurnRejected (rando : IRando) =
        rando.Sample [
            "It's nobody's turn."
            "No game in progress."
            "The game is stopped."
        ]

    let justAdvancedQueueAcknowledged (rando : IRando) =
        rando.Sample [
            "Just advanced queue."
        ]

    let justStartedQueueAcknowledgments currentAskerMention currentAnswererMention =
        let descriptions : Printf.StringFormat<string -> string -> string> list = [
            "Just started queue. %s is asking %s."
        ]
        descriptions |> List.map (fun s -> sprintf s currentAskerMention currentAnswererMention)

    let justStartedQueueAcknowledged currentAskerMention currentAnswererMention (rando : IRando) =
        justStartedQueueAcknowledgments currentAskerMention currentAnswererMention |> rando.Sample

    let justStoppedQueueAcknowledged (rando : IRando) =
        rando.Sample [
            "Just stopped queue."
        ]

    let justShuffledQueueAcknowledged (rando : IRando) =
        rando.Sample [
            "Just shuffled queue."
        ]

    let nextTurnAcknowledged (rando : IRando) =
        rando.Sample [
            "Okey-dokey."
        ]

    let nextTurnGameStoppedRejected (rando : IRando) =
        rando.Sample [
            "I can't advance the queue when there is no game in progress!"
        ]

    let nextTurnNotModOrCurrentPlayerRejected (rando : IRando) =
        rando.Sample [
            "Only mods or current asker/answerer can advance the queue."
        ]

    let askerHasntSaidAnythingReminder (rando : IRando) =
        rando.Sample [
            "You haven't said anything in a while... Thinking of a good dare?"
        ]

    let answererHasntSaidAnythingReminder (rando : IRando) =
        rando.Sample [
            "You haven't said anything in a while... Are you still there?"
        ]

    let askerAutoSkippedReminder (rando : IRando) =
        rando.Sample [
            "You haven't said anything in too long, so you have been skipped and removed from the game!"
        ]

    let answererAutoSkippedReminder (rando : IRando) =
        rando.Sample [
            "You haven't said anything in too long, so you have been skipped and removed from the game!"
        ]