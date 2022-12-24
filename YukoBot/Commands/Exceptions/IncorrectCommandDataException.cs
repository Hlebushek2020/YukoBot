using DSharpPlus.Entities;
using System;
using System.Reflection.Metadata;
using YukoBot.Extensions;

namespace YukoBot.Commands.Exceptions
{
    internal class IncorrectCommandDataException : Exception
    {
        public IncorrectCommandDataException(string message) : base(message)
        {
        }

        public DiscordEmbedBuilder ToDiscordEmbed(string title) =>
            new DiscordEmbedBuilder().WithSadMessage(title, Message);

        public DiscordEmbedBuilder ToDiscordEmbed() =>
            new DiscordEmbedBuilder().WithDescription(Message).WithColor(Constants.ErrorColor);
    }
}