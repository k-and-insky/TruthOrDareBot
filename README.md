# TruthOrDareBot

TruthOrDareBot is a Discord bot written in F#, that runs on Mono 4.4.2, using Discord.Net and FSharp.Control.Reactive.

## Setup

### Ubuntu 16.04

All in one command:

```
apt-get update &&
apt-get -y install mono-complete fsharp curl &&
curl -LO https://dist.nuget.org/win-x86-commandline/latest/nuget.exe &&
yes | certmgr -ssl https://discordapp.com &&
yes | certmgr -ssl https://gateway.discord.gg &&
mono nuget.exe install -OutputDirectory packages &&
xbuild /p:Configuration=Release TruthOrDareBot.fsproj &&
mono bin/Release/TruthOrDareBot.exe
```

### General

- Install Xamarin Studio, or at least xbuild and nuget
- If you want to run the app locally, install Mono 4.4.2 (4.6+ will throw a SocketException due to this bug in Discord.Net: https://github.com/RogueException/Discord.Net/issues/297)
- Otherwise, you can run the app via Docker by installing Docker and docker-compose
- Install Discord SSL certs via `certmgr -ssl https://discordapp.com && certmgr -ssl https://gateway.discord.gg`
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

### Environment Variables

- `TRUTH_OR_DARE_BOT_TOKEN`: The bot auth token
- `TRUTH_OR_DARE_BOT_SERVER`: The name of the server to listen on (in case the bot is registered to multiple servers); defaults to "truth-or-dare-bot-test"
- `TRUTH_OR_DARE_BOT_CHANNEL`: The name of the channel to listen on; defaults to "truth-or-dare"
- `TRUTH_OR_DARE_BOT_MOD_ROLES`: The semicolon-delimited list of roles allowed to moderate the bot; defaults to "Administrators; Moderators; Staff; Ambassadors"
- `TRUTH_OR_DARE_BOT_MINIMUM_PLAYERS`: The minimum number of players to wait for before starting the game; defaults to "4"
- `TRUTH_OR_DARE_BOT_REMINDER_MINUTES`: The number of minutes to wait after a turn starts before pinging players who have remained silent; defaults to "3"
- `TRUTH_OR_DARE_BOT_AUTO_SKIP_MINUTES`: The number of minutes to wait after a turn starts before removing players who have remained silent; defaults to "5"
- `TRUTH_OR_DARE_CUTE_MODE`: Enables some cutesy commands; defaults to "false"

### Notes

- Make sure that dependency DLLs are in the same directory as the binary.
- There is no real error logging. Caught errors are written to stdout, but the Discord.Net package generally does a piss-poor job of catching them, so the bot often fails silently.
- The bot will message "Bot started" on startup, when it finds the matching channel.
