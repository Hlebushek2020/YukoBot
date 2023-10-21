using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace YukoBot.Extensions
{
    public static class DiscordExtension
    {
        private static IEnumerable<string> GetAllLinks(this DiscordMessage message)
        {
            return message.Attachments.Select(x => x.Url)
                .Concat(
                    message.Embeds.Where(x => x.Url != null)
                        .Select(x => x.Url.ToString()))
                .Concat(
                    message.Embeds.Where(x => x.Image?.Url != null)
                        .Select(x => x.Image.Url.ToString()));
        }

        public static IEnumerable<string> GetImages(this DiscordMessage message, IYukoSettings yukoSettings) =>
            message.GetAllLinks().Where(x => !yukoSettings.Filters.Any(y => Regex.Match(x, y).Success)).ToHashSet();

        public static bool HasImages(this DiscordMessage message, IYukoSettings yukoSettings) =>
            message.GetAllLinks().Any(x => !yukoSettings.Filters.Any(y => Regex.Match(x, y).Success));

        public static List<DiscordMessage> ToList(this DiscordMessage discordMessage) =>
            new List<DiscordMessage> { discordMessage };

        public static DiscordEmbedBuilder WithHappyTitle(this DiscordEmbedBuilder discordEmbedBuilder, string title) =>
            discordEmbedBuilder.WithTitle($"{title} {Constants.HappySmile}");

        public static DiscordEmbedBuilder WithSadTitle(this DiscordEmbedBuilder discordEmbedBuilder, string title) =>
            discordEmbedBuilder.WithTitle($"{title} {Constants.SadSmile}");

        public static DiscordEmbedBuilder WithHappyMessage(
            this DiscordEmbedBuilder discordEmbedBuilder,
            string title,
            string description) =>
            discordEmbedBuilder.WithTitle($"{title} {Constants.HappySmile}")
                .WithColor(Constants.SuccessColor)
                .WithDescription(description);

        public static DiscordEmbedBuilder WithSadMessage(
            this DiscordEmbedBuilder discordEmbedBuilder,
            string title,
            string description) =>
            discordEmbedBuilder.WithTitle($"{title} {Constants.SadSmile}")
                .WithColor(Constants.ErrorColor)
                .WithDescription(description);

        public static string GetLocalizedDescription(this Command command) =>
            GetLocalizedDescription(command.Description);

        public static string GetLocalizedDescription(this CommandArgument commandArgument) =>
            GetLocalizedDescription(commandArgument.Description);

        private static string GetLocalizedDescription(string key)
        {
            string localizedDescription = Resources.ResourceManager.GetString(key);
            return string.IsNullOrWhiteSpace(localizedDescription) ? key : localizedDescription;
        }
    }
}