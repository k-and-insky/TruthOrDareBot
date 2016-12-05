FROM mono:4.4.2

WORKDIR /usr/src/app

RUN yes | certmgr -ssl https://discordapp.com

RUN yes | certmgr -ssl https://gateway.discord.gg

CMD ["mono", "TruthOrDareBot.exe"]
