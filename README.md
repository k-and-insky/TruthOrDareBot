# TruthOrDareBot

TruthOrDareBot is a Discord bot written in F#, that runs on Mono 4.4.2, using Discord.Net and FSharp.Control.Reactive.

## Setup

- Install Xamarin Studio, or at least xbuild and nuget
- If you want to run the app locally, install Mono 4.4.2 (4.6+ will throw a SocketException due to this bug in Discord.Net: https://github.com/RogueException/Discord.Net/issues/297)
- Otherwise, you can run the app via Docker by installing Docker and docker-compose
- Install the packages via Xamarin Studio or nuget
- Build the app
- Run the app via mono or `docker-compose up` (`docker-compose up -d` to detach)

## Development

To contribute, either add an issue or fork (or branch, for direct collaborators) and submit a topic PR.

The functional style here is fairly loose, but a couple of philosophical decisions are set in stone:

- There is (in theory) a formal separation between the Discord-aware, network-oriented "Client" and the Discord-agnostic, logic-oriented "Game".

- Transfers between the Client and Game are represented by Requests and Responses.
Right now they are simple function arguments and return values, but the philosophical basis is even the Reminders could be implemented as Responses,
and they could arrive out-of-band; instead of having a Response-handler in every Client method, there would be one match-with Response handler that subscribes to the Response Observable.
But for now, this is close enough. The Reminders being represented as an Event is close enough for now, although the incoming Observable (the channel messages) does break the Discord-agnosticism of the Game.

- The state of the game is represented as one whole value that can be recalculated at any moment. *When* it is recalculated is important, because it also determines what the latest state transitions are as well.

## Customization

- The replies can be totally customized, and most of them rotate through multiple strings. They are in `Replies.fs`.
- There are some constants in `Program.fs` for things like `minimumPlayers`

## Deployment

- The auth token is based on an environment variable, `TRUTH_OR_DARE_BOT_TOKEN`
- Make sure that dependency DLLs are in the same directory as the binary.
- There is no real error logging. Caught errors are written to stdout, but the Discord.Net package generally does a piss-poor job of catching them, so the bot often fails silently.
- The bot will message "Bot started" on startup, when it finds the matching channel.
