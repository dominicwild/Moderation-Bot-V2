using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ModerationBot.BotLib {
    class User {

        public string Username { get; set; }
        public string Host { get; set; }
        public string RealName { get; set; }
        public string IP { get; set; }

        public User(string hostString) {
            string[] components = hostString.Split(new char[] { '!', '@' });
            this.Username = components[0];
            this.RealName = components[1];
            this.Host = components[2];
            this.IP = this.GetIP(this.Host);
        }

        private string GetIP(string host) {
            Match match = Regex.Match(host, @"\d{1,3}.\d{1,3}.\d{1,3}.\d{1,3}"); //Matches IPv4 address
            string ip = "IP not detected.";
            if (match.Success) {
                ip = match.Value;
            }
            return ip;
        }

        public override string ToString() {
            return this.Username;
        }
    }
}
