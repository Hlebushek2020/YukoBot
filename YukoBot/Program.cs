using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using YukoBot.Models.Log;
using YukoBot.Models.Log.Providers;
using YukoBot.Settings;

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
        }

        static async Task<int> Main(string[] args)
        {
            Console.Title = "Yuko [Bot]";

            if (!YukoSettings.Availability())
            {
                return 0;
            }

            try
            {
                using (YukoBot yuko = YukoBot.Current)
                {
                    yuko.RunAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                ILogger defaultLogger = YukoLoggerFactory.Current.CreateLogger<DefaultLoggerProvider>();
                defaultLogger.LogCritical(new EventId(0, "App"), ex, "");
                return 1;
            }

            return 0;
        }
    }
}