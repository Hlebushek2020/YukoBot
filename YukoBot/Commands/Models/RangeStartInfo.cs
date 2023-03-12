using DSharpPlus.Entities;

namespace YukoBot.Commands.Models
{
    internal class RangeStartInfo
    {
        public DiscordMessage StartMessage { get; }
        public DiscordChannel Channel { get; }

        public RangeStartInfo(DiscordMessage startMessage, DiscordChannel channel)
        {
            StartMessage = startMessage;
            Channel = channel;
        }
    }
}