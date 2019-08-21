using ModerationBot.BotLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace ModerationBot {
    class BotEventManager : EventManager {

        public BotEventManager(IrcBot bot) : base(bot) { }

        public override void OnWelcome() {
            bot.Join(bot.Channel);
            bot.Message(bot.Channel, "Hello, World!");
        }

        public override void OnDirectMessage(User sender, string message) {
            bot.Message(sender, "Hello, thank you for talking to me.");
        }

        public override void OnChannelMessage(User sender, string channel, string message) {
            bot.Message(channel, $"Echoing from {sender}: {message}");
        }

    }
}
