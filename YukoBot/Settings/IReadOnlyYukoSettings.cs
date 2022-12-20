﻿namespace YukoBot.Interfaces
{
    public interface IReadOnlyYukoSettings
    {
        #region Propirties
        public string DatabaseHost { get; }
        public string DatabaseUser { get; }
        public string DatabasePassword { get; }
        public int[] DatabaseVersion { get; }
        public string BotToken { get; }
        public string BotPrefix { get; }
        public string BotDescription { get; }
        public bool BugReport { get; }
        public ulong BugReportChannel { get; }
        public ulong BugReportServer { get; }
        public string ServerAddress { get; }
        public string ServerInternalAddress { get; }
        public int ServerPort { get; }
        public string ClientActualApp { get; }
        public int DiscordMessageLimit { get; }
        public int DiscordMessageLimitSleepMs { get; }
        #endregion

        public void SetApp(string appLink);
    }
}