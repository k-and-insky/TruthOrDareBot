FROM ubuntu:16.04

WORKDIR /usr/src/app

RUN apt-get update

RUN apt-get -y install mono-complete nuget fsharp

RUN yes | certmgr -ssl https://discordapp.com

RUN yes | certmgr -ssl https://gateway.discord.gg

RUN nuget install

RUN xbuild /p:Configuration=Release

CMD ["mono", "bin/Release/TruthOrDareBot.exe"]
