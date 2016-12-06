FROM ubuntu:16.04

WORKDIR /usr/src/app

RUN apt-get update

RUN apt-get -y install mono-complete fsharp curl

RUN curl -LO https://dist.nuget.org/win-x86-commandline/latest/nuget.exe

RUN yes | certmgr -ssl https://discordapp.com

RUN yes | certmgr -ssl https://gateway.discord.gg

RUN mono nuget.exe install -OutputDirectory packages

RUN xbuild /p:Configuration=Release

CMD ["mono", "bin/Release/TruthOrDareBot.exe"]
