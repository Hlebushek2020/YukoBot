using DSharpPlus.Entities;
using System;

namespace YukoBot.Commands.Exceptions
{
    internal class IncorrectCommandDataException : Exception
    {
        public IncorrectCommandDataException(string message) : base(message) { }

        public DiscordEmbedBuilder ToDiscordEmbed(string title)
        {
            return new DiscordEmbedBuilder()
                .WithTitle(title)
                .WithColor(Constants.ErrorColor)
                .WithDescription($"{Message} {Constants.SadSmile}");
        }

        public DiscordEmbedBuilder ToDiscordEmbed()
        {
            return new DiscordEmbedBuilder()
                .WithColor(Constants.ErrorColor)
                .WithDescription($"{Message} {Constants.SadSmile}");
        }
    }
}