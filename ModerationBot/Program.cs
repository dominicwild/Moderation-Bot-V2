using ModerationBot.BotLib;
using System;

namespace ModerationBot {
    class Program {
        static void Main(string[] args) {
            //IrcBot bot = new IrcBot("irc.freenode.net","TestoBot");
            IrcBot bot = new IrcBot(new Configuration("../../../config.csv"));
            bot.EventManager = new BotEventManager(bot);
            bot.Connect();

            Console.WriteLine(System.Environment.CurrentDirectory);
            Configuration config = new Configuration("config.csv");

            bot.PrintState(); //Loops forever
        }
    }
}
