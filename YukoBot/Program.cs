using Microsoft.Extensions.Logging;
using System;
using YukoBot.Models.Log;
using YukoBot.Models.Log.Providers;
using YukoBot.Settings;

namespace YukoBot
{
    class Program
    {
        static int Main(string[] args)
        {
            Console.Title = "Yuko[Bot]";

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