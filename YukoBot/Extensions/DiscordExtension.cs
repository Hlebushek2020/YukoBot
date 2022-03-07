using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;

namespace YukoBot.Extensions
{
    public static class DiscordExtension
    {
        public static IEnumerable<string> GetImages(this DiscordMessage message)
        {
            return message.Attachments.Select(x => x.Url).Concat(
                message.Embeds.Where(x => x.Url != null).Select(x => x.Url.ToString()));
        }

        public static bool HasImages(this DiscordMessage message)
        {
            return message.Attachments.Count > 0 || message.Embeds.Where(x => x.Url != null).Count() > 0;
        }
    }
}
