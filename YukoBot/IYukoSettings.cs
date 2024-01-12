﻿using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace YukoBot
{
    public interface IYukoSettings
    {
        #region Propirties
        string DatabaseHost { get; }
        string DatabaseUser { get; }
        string DatabasePassword { get; }
        int[] DatabaseVersion { get; }
        string BotToken { get; }
        string BotPrefix { get; }
        LogLevel BotLogLevel { get; }
        bool BugReport { get; }
        ulong BugReportChannel { get; }
        ulong BugReportServer { get; }
        string ServerAddress { get; }
        string ServerInternalAddress { get; }
        int ServerPort { get; }
        string ClientActualApp { get; }
        int NumberOfMessagesPerRequest { get; }
        int IntervalBetweenMessageRequests { get; }
        int UserTokenExpirationTime { get; }
        int UserTokenRemovalTime { get; }
        IReadOnlyList<string> Filters { get; }
        #endregion

        void SetApp(string appLink);
    }
}