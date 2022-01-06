﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

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
        public static string ProgramResourceFolder { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SergeyGovorunov", "YukoClient(DFLC)");

        public string Theme { get; set; } = "Light";
        public string Host { get; set; } = "127.0.0.1";
        public int Port { get; set; } = 10000;
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
            return File.Exists(Path.Combine(ProgramResourceFolder, "settings.json"));
        }
    }
}