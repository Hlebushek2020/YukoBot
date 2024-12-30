using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace YukoClientBase.Models
{
    public class Settings
    {
        public const string YukoClientMutexName = "YukoClientMutex";
        public const string FakePassword = "FakePassword";
        public const string ServersCacheFile = "servers.json";

        #region Static members
        [JsonIgnore]
        public static string ProgramResourceFolder { get; }

        private static readonly string _settingsFilePath;
        private static readonly string _loginFilePath;

        static Settings()
        {
            ProgramResourceFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "SergeyGovorunov",
                "YukoClient(DFLC)");

            _settingsFilePath = Path.Combine(ProgramResourceFolder, "settings.json");
            _loginFilePath = Path.Combine(ProgramResourceFolder, ".login");
        }
        #endregion

        #region Instance
        private static Settings _settings;

        [JsonIgnore]
        public static Settings Current
        {
            get
            {
                if (_settings != null)
                    return _settings;

                _settings = File.Exists(_settingsFilePath)
                    ? JsonConvert.DeserializeObject<Settings>(File.ReadAllText(_settingsFilePath, Encoding.UTF8))
                    : new Settings();

                return _settings;
            }
        }
        #endregion

        public Themes Theme { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public int MaxDownloadThreads { get; set; }

        private Settings()
        {
            Theme = Themes.Light;
            Host = "127.0.0.1";
            Port = 10000;
            MaxDownloadThreads = 4;
        }

        public void Save()
        {
            Directory.CreateDirectory(ProgramResourceFolder);
            using (StreamWriter streamWriter = new StreamWriter(_settingsFilePath, false, Encoding.UTF8))
                streamWriter.Write(JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        public static bool Availability() => File.Exists(_settingsFilePath);

        #region Protected Data
        public static void LoadLoginData(out string login, out byte[] protectedData)
        {
            login = null;
            protectedData = null;

            if (!File.Exists(_loginFilePath))
                return;

            using (BinaryReader binaryReader = new BinaryReader(
                       new FileStream(_loginFilePath, FileMode.Open), Encoding.UTF8, false))
            {
                login = binaryReader.ReadString();
                int count = binaryReader.ReadInt32();
                protectedData = binaryReader.ReadBytes(count);
            }
        }

        public static void SaveLoginData(string login, byte[] protectedData)
        {
            using (BinaryWriter binaryWriter = new BinaryWriter(
                       new FileStream(_loginFilePath, FileMode.Create), Encoding.UTF8, false))
            {
                binaryWriter.Write(login);
                binaryWriter.Write(protectedData.Length);
                binaryWriter.Write(protectedData);
            }
        }

        public static void DeleteLoginData()
        {
            if (File.Exists(_loginFilePath))
                File.Delete(_loginFilePath);
        }
        #endregion
    }
}