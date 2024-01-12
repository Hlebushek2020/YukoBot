using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace YukoBot
{
    public class YukoSettings : IYukoSettings
    {
        private static readonly string _settingsPath = Path.Combine(Program.Directory, "settings.json");

        #region Propirties
        public string DatabaseHost { get; set; }
        public string DatabaseUser { get; set; }
        public string DatabasePassword { get; set; }
        public int[] DatabaseVersion { get; set; }
        public string BotToken { get; set; }
        public string BotPrefix { get; set; }
        public bool BugReport { get; set; } = false;
        public ulong BugReportChannel { get; set; }
        public ulong BugReportServer { get; set; }
        public string ServerAddress { get; set; }
        public string ServerInternalAddress { get; set; }
        public int ServerPort { get; set; }
        public string ClientActualApp { get; set; }
        public int NumberOfMessagesPerRequest { get; set; }
        public int IntervalBetweenMessageRequests { get; set; }
        public int UserTokenExpirationTime { get; set; }
        public int UserTokenRemovalTime { get; set; }
        public LogLevel BotLogLevel { get; set; }
        public IReadOnlyList<string> Filters { get; set; }
        #endregion

        public YukoSettings()
        {
            BotPrefix = "yuko!";
            BotLogLevel = LogLevel.Information;
            NumberOfMessagesPerRequest = 10;
            IntervalBetweenMessageRequests = 500;
            UserTokenExpirationTime = 20;
            UserTokenRemovalTime = 25;
        }

        public void SetApp(string appLink)
        {
            ClientActualApp = appLink;
            Save(this);
        }

        /// <summary>
        /// Loads settings from configuration file
        /// </summary>
        /// <returns>Read only settings</returns>
        public static IYukoSettings Load() =>
            JsonConvert.DeserializeObject<YukoSettings>(File.ReadAllText(_settingsPath, Encoding.UTF8));

        private static void Save(YukoSettings yukoSettings)
        {
            using StreamWriter streamWriter = new StreamWriter(_settingsPath, false, Encoding.UTF8);
            streamWriter.Write(JsonConvert.SerializeObject(yukoSettings, Formatting.Indented));
        }

        /// <summary>
        /// Checks for the existence of a configuration file. If the configuration file does not exist, it will be created.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration file exists</returns>
        public static bool Availability()
        {
            if (File.Exists(_settingsPath))
                return true;

            Save(new YukoSettings());

            return false;
        }
    }
}