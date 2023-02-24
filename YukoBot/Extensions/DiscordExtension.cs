using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace YukoBot.Extensions
{
    public static class DiscordExtension
    {
        /*
        private static readonly ILogger _defaultLogger =
            YukoLoggerFactory.Current.CreateLogger<DefaultLoggerProvider>();

        private static readonly EventId _getImagesEventId =
            new EventId(0, $"{nameof(DiscordExtension)}: {nameof(GetImages)}");
        private static readonly EventId _hasImagesEventId =
            new EventId(0, $"{nameof(DiscordExtension)}: {nameof(HasImages)}");
        */

        private static IEnumerable<string> GetAllImages(DiscordMessage message)
        {
            return message.Attachments.Select(x => x.Url)
                .Concat(message.Embeds.Where(x => x.Url != null)
                    .Select(x => x.Url.ToString()))
                .Concat(message.Embeds.Where(x => x.Image?.Url != null)
                    .Select(x => x.Image.Url.ToString()));
        }

        public static IEnumerable<string> GetImages(this DiscordMessage message)
        {
            return GetAllImages(message).Where(x => !Settings.YukoSettings.Current.Filters
                .All(y => Regex.Match(x, y).Success)).ToHashSet();
        }

        public static bool HasImages(this DiscordMessage message)
        {
            return GetAllImages(message)
                .Any(x => !Settings.YukoSettings.Current.Filters
                    .All(y => Regex.Match(x, y).Success));
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

        public static DiscordEmbedBuilder WithHappyMessage(this DiscordEmbedBuilder discordEmbedBuilder, string title,
            string description) =>
            discordEmbedBuilder.WithTitle($"{title} {Constants.HappySmile}").WithColor(Constants.SuccessColor)
                .WithDescription(description);

        public static DiscordEmbedBuilder WithSadMessage(this DiscordEmbedBuilder discordEmbedBuilder, string title,
            string description) =>
            discordEmbedBuilder.WithTitle($"{title} {Constants.SadSmile}").WithColor(Constants.ErrorColor)
                .WithDescription(description);
    }
}