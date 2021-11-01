using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace ModerationBot.BotLib {
    class Configuration {

        private Dictionary<string, string> configurationData;
        private string fileName;

        public Configuration(string fileName) {
            this.fileName = fileName;
            configurationData = new Dictionary<string, string>();
            InitConfigurationData();
        }

        private void InitConfigurationData() {
            if (File.Exists(this.fileName)) {
                StreamReader reader = new StreamReader(fileName);
                string[] contents = reader.ReadToEnd().Split(",");
                foreach (string info in contents) {
                    string[] keyValue = info.Split("=");
                    if (keyValue.Length == 2) { //Ignore anything that doesn't conform to configName=configValue
                        configurationData.Add(processKey(keyValue[0]), keyValue[1].Trim());
                    }
                }
            } else {
                throw new FileNotFoundException();
            }
        }

        public string GetString(string key) {
            key = processKey(key);
            return this.configurationData.ContainsKey(key) ? this.configurationData[key] : "";
        }

        public int GetInt(string key) {
            key = processKey(key);
            return this.configurationData.ContainsKey(key) ? Int32.Parse(this.configurationData[key]) : -1;
        }

        private string processKey(string key) {
            return key.ToLower().Trim();
        }

    }
}
