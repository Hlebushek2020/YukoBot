using DSharpPlus.Entities;

namespace YukoBot
{
    internal sealed class Constants
    {
        public static DiscordColor ErrorColor { get; } = DiscordColor.Red;
        public static DiscordColor SuccessColor { get; } = DiscordColor.Orange;
        public static DiscordColor StatusColor { get; } = DiscordColor.DarkGray;
        public static DiscordColor PremiumColor { get; } = DiscordColor.Gold;

        public const string HappySmile = "(≧◡≦)";
        public const string SadSmile = "(⋟﹏⋞)";

        public const string DeleteMessageEmoji = ":negative_squared_cross_mark:";
    }
}
