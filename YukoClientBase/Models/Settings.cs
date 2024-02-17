using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Sergey.UI.Extension.Themes;

namespace YukoClientBase.Models
{
    public class Settings
    {
        public const string YukoClientMutexName = "YukoClientMutex";
        public const string ServersCacheFile = "servers.json";

        #region Instance
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
        #endregion

        [JsonIgnore]
        public static string ProgramResourceFolder { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SergeyGovorunov", "YukoClient(DFLC)");

        public Theme Theme { get; set; } = Theme.Light;
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 10000;
        public int MaxDownloadThreads { get; set; } = 4;

        public void Save()
        {
            Directory.CreateDirectory(ProgramResourceFolder);
            using (StreamWriter streamWriter = new StreamWriter(Path.Combine(ProgramResourceFolder, "settings.json"),
                       false, Encoding.UTF8))
            {
                streamWriter.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
            }
        }

        public static bool Availability() { return File.Exists(Path.Combine(ProgramResourceFolder, "settings.json")); }

       
        public static void LoadLoginData(out string login, out byte[] protectedData)
        {
            login = null;
            protectedData = null;

            string loginData = Path.Combine(ProgramResourceFolder, ".login");

            if (!File.Exists(loginData))
                return;

            using (BinaryReader binaryReader = new BinaryReader(
                       new FileStream(loginData, FileMode.Open), Encoding.UTF8, false))
            {
                login = binaryReader.ReadString();
                int count = binaryReader.ReadInt32();
                protectedData = binaryReader.ReadBytes(count);
            }
        }

        public static void SaveLoginData(string login, byte[] protectedData)
        {
            string loginData = Path.Combine(ProgramResourceFolder, ".login");
            using (BinaryWriter binaryWriter = new BinaryWriter(
                       new FileStream(loginData, FileMode.Create), Encoding.UTF8, false))
            {
                binaryWriter.Write(login);
                binaryWriter.Write(protectedData.Length);
                binaryWriter.Write(protectedData);
            }
        }

        public static void DeleteLoginData()
        {
            string loginData = Path.Combine(ProgramResourceFolder, ".login");
            if (File.Exists(loginData))
                File.Delete(loginData);
        }
    }
}