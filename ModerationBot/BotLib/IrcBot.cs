using ModerationBot.BotLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace ModerationBot {
    class IrcBot {

        //Connection state variables
        private string server;
        private int port = 6667;
        private string gecos;
        private string nick;
        private string password;

        private Dictionary<string, string> supported;

        //Ping variables
        private long lastPing = 0;
        private int timeOut = 15; //Amount of time when TcpClient timesout
        private bool hasReplied = true;
        private Thread connectionThread;
        private bool connected = false;

        private string ident;
        public string Channel { get; set; }
        public bool AutoReconnect { get; set; } = true;
        public int ReconnectDelay { get; set; } = 3; //Delay in seconds

        //Objects for interacting with server
        private TcpClient client;
        private StreamWriter writer;
        private StreamReader reader;
        private Thread streamThread;
        private Configuration configuration;

        public EventManager EventManager { get; set; }

        public IrcBot(Configuration configuration) {
            this.configuration = configuration;
            this.server = configuration.GetString("server");
            this.nick = configuration.GetString("nick");
            this.gecos = configuration.GetString("gecos");
            this.ident = configuration.GetString("ident");
            this.port = configuration.GetInt("port");
            this.password = configuration.GetString("password");
            this.Channel = configuration.GetString("channel");

            this.EventManager = new DefaultEventManager(this);
        }

        private void initTcpClient() {
            this.client = new TcpClient();
            this.client.ReceiveTimeout = this.timeOut * 1000;
            this.supported = new Dictionary<string, string>();
        }

        public bool Connect() {
            //Set up TCP Client and Read/Write Streams
            initTcpClient();
            this.client.Connect(this.server, this.port);
            NetworkStream stream = client.GetStream();
            this.writer = new StreamWriter(stream);
            this.reader = new StreamReader(stream);
            //Send necessary IRC messages to establish a session
            this.Write(
                $"PASS {password}",
                $"USER {ident} * 8 {gecos}",
                $"NICK {nick}"
                );

            Console.WriteLine($"Successfully connected to {this.server} on port {this.port}");
            if (this.streamThread == null) {
                this.streamThread = new Thread(new ThreadStart(Run));
                streamThread.Start();
            }

            if (this.connectionThread == null) {
                this.connectionThread = new Thread(new ThreadStart(ConnectionManager));
                this.connectionThread.Start();
            }
            return true;
        }

        public void PrintState() {
            while (true) {
                Thread.Sleep(1000);
                Console.WriteLine(this.GetState(this.client));
            }
        }

        public TcpState GetState(TcpClient tcpClient) {
            TcpConnectionInformation state = null;
            try {
                state = IPGlobalProperties.GetIPGlobalProperties()
                  .GetActiveTcpConnections()
                  .SingleOrDefault(x => x.LocalEndPoint.Equals(tcpClient.Client.LocalEndPoint));
            } catch (Exception) { }
            return state != null ? state.State : TcpState.Unknown;
        }

        public void Run() {

            // identify with the server so your bot can be an op on the channel
            //writer.WriteLine($"PRIVMSG NickServ :IDENTIFY {nick} {password}");
            bool initialConnect = true;

            while (this.AutoReconnect || initialConnect) {
                initialConnect = false;
                try {
                    this.ConnectionLoop();
                } catch (Exception ex) when (ex is SocketException || ex is IOException) {
                    Console.WriteLine("Disconnected");
                    if (AutoReconnect) {
                        this.connected = false;
                        int attempts = 0;
                        while (connected == false) { //Keep trying to connect until we connect
                            Thread.Sleep(this.ReconnectDelay * 1000);
                            attempts++;
                            Console.WriteLine($"Reconnection attempt: {attempts}");
                            connected = this.Connect();
                        }
                    }
                }

            }

        }

        private void ConnectionLoop() {
            while (client.Connected) {
                string data = reader.ReadLine();
                if (data != null) {
                    Console.WriteLine(data);
                    ProcessData(data);
                }
            }
        }

        private void ProcessData(string data) {

            string[] d = data.Split(' ');

            if (d[0] == "PING") {
                Write("PONG");
            }

            if (d.Length > 1) {

                if (int.TryParse(d[1], out int code)) {
                    switch ((Code)code) { //Process the response code
                        case Code.RPL_WELCOME: {
                                EventManager.OnWelcome();
                                this.connected = true;
                                break;
                            }
                        case Code.ERR_NICKNAMEINUSE: {
                                ChangeNick(this.nick + '_');
                                break;
                            }
                        case Code.RPL_ISUPPORT: {
                                LogSupportData(data);
                                break;
                            }
                    }
                } else { //If it's not a response code, it is a command string
                    switch (d[1]) {
                        case "PRIVMSG": {
                                this.HandlePrivateMessage(data);
                                break;
                            }
                        case "PONG": {
                                this.hasReplied = true;
                                this.lastPing = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                break;
                            }
                    }
                }
            }
        }

        private void ConnectionManager() {
            while (true) {
                try {
                    if (this.client.Connected) {
                        Thread.Sleep(this.timeOut * 1000 / 2);
                        Write($"PING {this.server}");
                    }
                } catch (Exception) {
                    Console.WriteLine($"Failed to send PING to {this.server}");
                }
            }
        }

        private void LogSupportData(string data) {
            Match match = Regex.Match(data, @"[^\s]+?=[^\s]+");

            while (match.Success) {
                string[] supportedData = match.Value.Split("=");
                string supportKey = supportedData[0];
                string supportValue = supportedData[1];
                supported.Add(supportKey, supportValue);
                match = match.NextMatch();
            }
        }

        private void HandlePrivateMessage(string data) {
            string[] d = data.Split(" ");
            if (d.Length > 2) {
                User user = new User(d[0].Substring(1));

                string message = data.Split(':')[2];
                string target = d[2];
                if (target == nick) { // Someone sent a private message to the bot
                    EventManager.OnDirectMessage(user, message);
                } else {
                    EventManager.OnChannelMessage(user, target, message);
                }
            }
        }

        public void Join(string channel) {
            Write($"JOIN {channel}");
        }

        public void Message(string target, string message) {
            Write($"PRIVMSG {target} :{message}");
        }

        public void Message(User sender, string message) {
            this.Message(sender.Username, message);
        }

        public void ChangeNick(string newNick) {
            Write($"NICK {newNick}");
            this.nick = newNick;
        }

        private bool Write(params string[] messages) {
            foreach (string message in messages) { //Write to stream
                this.writer.WriteLine(message);
            }
            writer.Flush();
            foreach (string message in messages) { //Write to console
                Console.WriteLine(">" + message);
            }
            return true;
        }

    }
}

