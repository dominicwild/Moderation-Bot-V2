using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace ModerationBot.BotLib {
    class MaintenanceManager {

        private IrcBot bot;
        private int timeOut = 15; //Amount of time when TcpClient timesout in seconds
        private int step = 1; //Amount of delay between maintenance loops in seconds
        private int stepCount = 0; //The amount of time that has passed in seconds from the maintence thread starting
        private Thread thread;

        public MaintenanceManager(IrcBot bot) {
            this.bot = bot;
            this.thread = new Thread(new ThreadStart(ConnectionManager));
            this.thread.Start();
        }

        private void ConnectionManager() {
            while (true) {
                CheckConnectionStatus();
                Thread.Sleep(step * 1000);
                stepCount += step;
            }
        }

        private void CheckConnectionStatus() {
            try {
                if (bot.Client.Connected && stepCount % Math.Floor(timeOut/2.0) == 0) { //If bot doesn't recieve a message every x seconds, closes the connection
                    bot.Write($"PING {bot.Server}");
                }
            } catch (Exception) {
                Console.WriteLine($"Failed to send PING to {bot.Server}");
            }
        }

    }
}
