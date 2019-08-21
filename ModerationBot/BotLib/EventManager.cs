using System;
using System.Collections.Generic;
using System.Text;

namespace ModerationBot.BotLib {
    abstract class EventManager {

        protected IrcBot bot;

        public EventManager(IrcBot bot) {
            this.bot = bot;
        }

        public virtual void OnWelcome() { }

        public virtual void OnDirectMessage(User sender, string message) { }

        public virtual void OnChannelMessage(User sender, string channel, string message) { }
    }

    class DefaultEventManager : EventManager {

        public DefaultEventManager(IrcBot bot) : base(bot) { }

    }
}
