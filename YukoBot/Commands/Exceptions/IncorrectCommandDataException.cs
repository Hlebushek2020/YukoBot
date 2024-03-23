using System;
using DSharpPlus.Entities;
using YukoBot.Extensions;

namespace YukoBot.Commands.Exceptions
{
    internal class IncorrectCommandDataException : Exception
    {
        public IncorrectCommandDataException(string message) : base(message) { }

        public DiscordEmbedBuilder ToDiscordEmbed(string title) =>
            new DiscordEmbedBuilder().WithSadMessage(title, Message);
    }
}