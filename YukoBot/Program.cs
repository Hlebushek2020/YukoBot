using Microsoft.Extensions.Logging;
using System;
using YukoBot.Models.Log;

namespace YukoBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Yuko[Bot]";

            if (!YukoSettings.Availability())
            {
                return;
            }

            try
            {
                using (YukoBot yukoBot = YukoBot.Current)
                {
                    yukoBot.RunAsync().GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                YukoLoggerFactory.GetInstance().CreateLogger<ServerLogger>().Log(LogLevel.Critical, "App", ex);
            }
        }
    }
}