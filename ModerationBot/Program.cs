using System;

namespace ModerationBot {
    class Program {
        static void Main(string[] args) {
            IrcBot bot = new IrcBot("irc.freenode.net","TestoBot");
            bot.EventManager = new BotEventManager(bot);
            bot.Connect();

            bot.PrintState(); //Loops forever
        }
    }
}
