﻿using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace YukoBot
{
    internal class Program
    {
        private const string LogOutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] [{SourceContext}] {Message}{NewLine}{Exception}";

        public static string Version { get; }
        public static string Directory { get; }

        static Program()
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            Directory = Path.GetDirectoryName(currentAssembly.Location) ?? string.Empty;
            AssemblyInformationalVersionAttribute informationalVersionAttribute =
                currentAssembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (informationalVersionAttribute != null)
            {
                Version = informationalVersionAttribute.InformationalVersion;
            }
            else
            {
                Version version = currentAssembly.GetName().Version;
                Version = $"{version.Major}.{version.Minor}.{version.Build}";
            }
            #if DEBUG
            Version += " [DEBUG]";
            #endif
        }

        static async Task<int> Main(string[] args)
        {
            if (!YukoSettings.Availability())
                return 0;

            IYukoSettings yukoSettings = YukoSettings.Load();

            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .WriteTo.Console(
                    outputTemplate: LogOutputTemplate,
                    theme: SystemConsoleTheme.Colored)
                .WriteTo.File(
                    path: Path.Combine(Directory, "logs", ".log"),
                    outputTemplate: LogOutputTemplate,
                    rollingInterval: RollingInterval.Day)
                .Enrich.FromLogContext();

            loggerConfiguration = yukoSettings.BotLogLevel switch
            {
                LogLevel.Critical => loggerConfiguration.MinimumLevel.Fatal(),
                LogLevel.Error => loggerConfiguration.MinimumLevel.Error(),
                LogLevel.Warning => loggerConfiguration.MinimumLevel.Warning(),
                LogLevel.Information => loggerConfiguration.MinimumLevel.Information(),
                _ => loggerConfiguration.MinimumLevel.Debug()
            };

            Log.Logger = loggerConfiguration.CreateLogger();

            bool isShutdown = false;
            while (!isShutdown)
            {
                try
                {
                    using YukoBot yuko = new YukoBot(yukoSettings);
                    await yuko.RunAsync();
                    isShutdown = true;
                }
                catch (Exception ex)
                {
                    Log.Logger.ForContext<Program>().Fatal(ex, "Bot failed. Bot restart after 2 minutes.");
                    Thread.Sleep(120000);
                }
            }

            return 0;
        }
    }
}