using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ModerationBot.BotLib {
    class MaintenanceManager {

        private IrcBot bot;
        private int connectionTimeOut = 15; //Amount of time when TcpClient timesout in seconds
        private int changeNickAttempt = 60; //Amount of time to attempt simplifying nick
        private int step = 1; //Amount of delay between maintenance loops in seconds
        private int stepCount = 0; //The amount of time that has passed in seconds from the maintence thread starting
        internal bool removeUnderscore = true;
        private Thread thread;

        public MaintenanceManager(IrcBot bot) {
            this.bot = bot;
            this.thread = new Thread(new ThreadStart(ConnectionManager));
            this.thread.Start();
        }

        private void ConnectionManager() {
            while (true) {
                CheckConnectionStatus();
                RemoveUnderscore();
                Thread.Sleep(step * 1000);
                stepCount += step;
            }
        }

        private void CheckConnectionStatus() {
            try {
                if (bot.Client.Connected && stepCount % Math.Floor(connectionTimeOut/2.0) == 0) { //If bot doesn't recieve a message every x seconds, closes the connection
                    bot.Write($"PING {bot.Server}");
                }
            } catch (Exception) {
                Console.WriteLine($"Failed to send PING to {bot.Server}");
            }
        }

        private void RemoveUnderscore() {
            if (removeUnderscore && stepCount % changeNickAttempt == 0) {
                string nick = bot.Nick; 
                int length = nick.Length;
                if(nick.EndsWith('_')) {
                    for(int i = length-1; i >= 0; i--) {
                        Console.WriteLine($"The letter {nick[i]} is at position {i}");
                        if (!nick[i].Equals('_')) { //If we stop on a non-underscore
                            string newNick = nick.Substring(0, i+1);
                            bot.ChangeNick(newNick);
                            return;
                        }
                    }
                }
            }
        }

    }
}
