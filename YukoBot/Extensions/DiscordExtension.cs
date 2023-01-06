﻿using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;

namespace YukoBot.Extensions
{
    public static class DiscordExtension
    {
        public static IEnumerable<string> GetImages(this DiscordMessage message)
        {
            return message.Attachments.Select(x => x.Url).Concat(
                message.Embeds.Where(x => x.Url != null).Select(x => x.Url.ToString())).Concat(
                message.Embeds.Where(x => x.Image?.Url != null).Select(x => x.Image.Url.ToString())).ToHashSet();
        }

        public static bool HasImages(this DiscordMessage message)
        {
            return message.Attachments.Count > 0 || message.Embeds.Where(x => x.Url != null || x.Image?.Url != null).Count() > 0;
        }

        public static List<DiscordMessage> ToList(this DiscordMessage discordMessage)
        {
            List<DiscordMessage> list = new List<DiscordMessage>
            {
                discordMessage
            };
            return list;
        }

        public static DiscordEmbedBuilder WithHappyTitle(this DiscordEmbedBuilder discordEmbedBuilder, string title) =>
            discordEmbedBuilder.WithTitle($"{title} {Constants.HappySmile}");

        public static DiscordEmbedBuilder WithSadTitle(this DiscordEmbedBuilder discordEmbedBuilder, string title) =>
            discordEmbedBuilder.WithTitle($"{title} {Constants.SadSmile}");

        public static DiscordEmbedBuilder WithHappyMessage(this DiscordEmbedBuilder discordEmbedBuilder, string title, string description) =>
            discordEmbedBuilder.WithTitle($"{title} {Constants.HappySmile}").WithColor(Constants.SuccessColor).WithDescription(description);

        public static DiscordEmbedBuilder WithSadMessage(this DiscordEmbedBuilder discordEmbedBuilder, string title, string description) =>
            discordEmbedBuilder.WithTitle($"{title} {Constants.SadSmile}").WithColor(Constants.ErrorColor).WithDescription(description);

    }
}