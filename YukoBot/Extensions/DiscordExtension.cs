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
                message.Embeds.Where(x => x.Image != null).Select(x => x.Image.Url.ToString()));
        }

        public static bool HasImages(this DiscordMessage message)
        {
            return message.Attachments.Count > 0 || message.Embeds.Where(x => x.Image != null).Count() > 0;
        }
    }
}
