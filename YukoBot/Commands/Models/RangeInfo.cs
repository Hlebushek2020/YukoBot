using DSharpPlus.Entities;

namespace YukoBot.Commands.Models
{
    internal class RangeInfo
    {
        public DiscordMessage StartMessage { get; }
        public DiscordChannel Channel { get; }

        public RangeInfo(DiscordMessage startMessage, DiscordChannel channel)
        {
            StartMessage = startMessage;
            Channel = channel;
        }
    }
}
