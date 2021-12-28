using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace YukoCollectionsClient.Models
{
    public class Settings
    {
        #region Json Ignore
        private static Settings settings;

        [JsonIgnore]
        public static Settings Current
        {
            get
            {
                if (settings != null)
                {
                    return settings;
                }
                string settingsFile = Path.Combine(ProgramResourceFolder, "settings.json");
                if (File.Exists(settingsFile))
                {
                    settings = JsonConvert.DeserializeObject<Settings>(File.ReadAllText(settingsFile, Encoding.UTF8));
                }
                else
                {
                    settings = new Settings();
                }
                return settings;
            }
        }

        [JsonIgnore]
        public static string ProgramResourceFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SergeyGovorunov", "YukoClient(DFLC)");
        #endregion

        public string Theme { get; set; } = "Light";
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 0;
        public int MaxDownloadThreads { get; set; } = 4;

        public void Save()
        {
            Directory.CreateDirectory(ProgramResourceFolder);
            using (StreamWriter streamWriter = new StreamWriter(Path.Combine(ProgramResourceFolder, "settings.json"), false, Encoding.UTF8))
            {
                streamWriter.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }

        public static bool Availability()
        {
            string settingsPath = Path.Combine(ProgramResourceFolder, "settings.json");

            if (File.Exists(settingsPath))
            {
                return true;
            }

            return false;
        }
    }
}