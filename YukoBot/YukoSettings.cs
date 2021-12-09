using Newtonsoft.Json;
using System.IO;
using System.Reflection;
using System.Text;

namespace YukoBot
{
    public class YukoSettings
    {
        #region Fields
        private static YukoSettings settings;
        private string clientActualApp = null;
        #endregion

        #region Propirties
        public string DatabaseHost { get; set; }
        public string DatabaseUser { get; set; }
        public string DatabasePassword { get; set; }
        public int[] DatabaseVersion { get; set; }
        public string BotToken { get; set; }
        public string ServerAddress { get; set; }
        public string ServerInternalAddress { get; set; }
        public int ServerPort { get; set; }
        public string ClientActualApp
        {
            get { return clientActualApp; }
            set
            {
                clientActualApp = value;
                string settingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");
                using (StreamWriter streamWriter = new StreamWriter(settingsPath, false, Encoding.UTF8))
                {
                    streamWriter.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
                }
            }
        }
        public int DiscordMessageLimit { get; set; } = 100;
        public int DiscordMessageLimitSleepMs { get; set; } = 1000;
        [JsonIgnore]
        public static YukoSettings Current
        {
            get
            {
                if (settings == null)
                {
                    string settingsFile = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");
                    settings = JsonConvert.DeserializeObject<YukoSettings>(File.ReadAllText(settingsFile, Encoding.UTF8));
                }
                return settings;
            }
        }
        #endregion

        public static bool Availability()
        {
            string settingsPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");

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
    }
}
