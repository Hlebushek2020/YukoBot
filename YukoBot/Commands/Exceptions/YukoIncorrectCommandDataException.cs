using DSharpPlus.Entities;
using System;

namespace YukoBot.Commands.Exceptions
{
    internal class YukoIncorrectCommandDataException : Exception
    {
        public YukoIncorrectCommandDataException(string message) : base(message) { }

        public DiscordEmbed ToDiscordEmbed(string title)
        {
            return new DiscordEmbedBuilder()
                .WithTitle(title)
                .WithColor(DiscordColor.Red)
                .WithDescription(Message + " (⋟﹏⋞)");
        }
    }
}