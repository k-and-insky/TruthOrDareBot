namespace TruthOrDareBot

open System
open Rando

module Replies =
    let addPlayerAcknowledgments =
        [
            "Roger that."
            "Alrighty!"
            "10-4, good buddy."
        ]

    let addPlayerAcknowledged() =
        sample addPlayerAcknowledgments

    let addPlayerRejections =
        [
            "You're already in the game, dingus."
        ]

    let addPlayerRejected() =
        sample addPlayerRejections

    let removePlayerSelfAcknowledgments =
        [
            "Aww, you're no fun."
            "Alright... Leave if you must."
        ]

    let removePlayerSelfAcknowledged() =
        sample removePlayerSelfAcknowledgments

    let removePlayerOtherAcknowledgments =
        [
            "Alright, I removed them, but I'm not happy about it."
            "For you, anything. *Poof!* They're gone."
        ]

    let removePlayerOtherAcknowledged() =
        sample removePlayerOtherAcknowledgments

    let removePlayerSelfNotInGameRejections =
        [
            "You're not in the game, silly!"
        ]

    let removePlayerSelfNotInGameRejected() =
        sample removePlayerSelfNotInGameRejections


    let removePlayerOtherNotInGameRejected() =
        sample [
            "Um, they're not in the game..."
        ]

    let removePlayerOtherNotModRejected() =
        sample [
            "Only mods can remove other players from the game."
        ]

    let stayedStoppedGameAcknowledged() =
        sample [
            "Still waiting for more players..."
        ]
    
    let stayedInProgressGameAcknowledged() =
        sample [
            "Game in progress."
        ]

    let justStartedGameAcknowledged() =
        sample [
            "Go time."
            "Alright chums, let's do this."
        ]

    let justStoppedGameAcknowledged() =
        sample [
            "Boo, the game is stopped. :("
        ]

    let waitingForGameStartAcknowledged() =
        sample [
            "Waiting for game to start."
        ]

    let showQueueRejected() =
        sample [
            "There is no queue to show!"
            "No game in progress."
            "The game is stopped."
        ]

    let whoseTurnRejected() =
        sample [
            "It's nobody's turn."
            "No game in progress."
            "The game is stopped."
        ]

    let justAdvancedQueueAcknowledged() =
        sample [
            "Just advanced queue."
        ]

    let justStartedQueueAcknowledged() =
        sample [
            "Just started queue."
        ]

    let justStoppedQueueAcknowledged() =
        sample [
            "Just stopped queue."
        ]

    let justShuffledQueueAcknowledged() =
        sample [
            "Just shuffled queue."
        ]

    let nextTurnAcknowledged() =
        sample [
            "Okey-dokey."
        ]

    let nextTurnGameStoppedRejected() =
        sample [
            "I can't advance the queue when there is no game in progress!"
        ]

    let nextTurnNotModOrCurrentPlayerRejected() =
        sample [
            "Only mods or current asker/answerer can advance the queue."
        ]

    let askerHasntSaidAnythingReminder() =
        sample [
            "You haven't said anything in a while... Thinking of a good dare?"
        ]

    let answererHasntSaidAnythingReminder() =
        sample [
            "You haven't said anything in a while... Are you still there?"
        ]

    let askerAutoSkippedReminder() =
        sample [
            "You haven't said anything in too long, so you have been skipped and removed from the game!"
        ]

    let answererAutoSkippedReminder() =
        sample [
            "You haven't said anything in too long, so you have been skipped and removed from the game!"
        ]