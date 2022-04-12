using System;

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

            using (YukoBot yukoBot = YukoBot.Current)
            {
                yukoBot.RunAsync().GetAwaiter().GetResult();
            }
        }
    }
}
