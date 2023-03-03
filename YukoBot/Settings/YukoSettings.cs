using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Logging;
using YukoBot.Interfaces;

namespace YukoBot.Settings
{
    public class YukoSettings : IReadOnlyYukoSettings
    {
        #region Propirties
        public string DatabaseHost { get; set; }
        public string DatabaseUser { get; set; }
        public string DatabasePassword { get; set; }
        public int[] DatabaseVersion { get; set; }
        public string BotToken { get; set; }
        public string BotPrefix { get; set; } = ">yuko";
        public bool BugReport { get; set; } = false;
        public ulong BugReportChannel { get; set; }
        public ulong BugReportServer { get; set; }
        public string ServerAddress { get; set; }
        public string ServerInternalAddress { get; set; }
        public int ServerPort { get; set; }
        public string ClientActualApp { get; set; }
        public int DiscordMessageLimit { get; set; } = 100;
        public int DiscordMessageLimitSleepMs { get; set; } = 1000;
        public int DiscordMessageLimitSleepMsDividerForOne { get; set; } = 10;
        public LogLevel DiscordApiLogLevel { get; set; } = LogLevel.Information;
        public LogLevel BotLogLevel { get; set; } = LogLevel.Information;
        public IReadOnlyList<string> Filters { get; set; }
        #endregion

        #region Instance
        private static YukoSettings _settings;

        [JsonIgnore]
        public static IReadOnlyYukoSettings Current
        {
            get
            {
                if (_settings == null)
                {
                    string settingsFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                        "settings.json");
                    _settings = JsonConvert.DeserializeObject<YukoSettings>(
                        File.ReadAllText(settingsFile, Encoding.UTF8));
                }
                return _settings;
            }
        }
        #endregion

        private void Save()
        {
            string settingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "settings.json");
            using (StreamWriter streamWriter = new StreamWriter(settingsPath, false, Encoding.UTF8))
            {
                streamWriter.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }

        public static bool Availability()
        {
            string settingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "settings.json");

            if (File.Exists(settingsPath))
            {
                return true;
            }

            using (StreamWriter streamWriter = new StreamWriter(settingsPath, false, Encoding.UTF8))
            {
                streamWriter.Write(JsonConvert.SerializeObject(new YukoSettings(), Formatting.Indented));
            }

            return false;
        }

        public void SetApp(string appLink)
        {
            ClientActualApp = appLink;
            Save();
        }
    }
}