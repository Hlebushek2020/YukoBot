using DSharpPlus.Entities;

namespace YukoBot
{
    internal sealed class Constants
    {
        public static DiscordColor ErrorColor { get; } = DiscordColor.Red;
        public static DiscordColor SuccessColor { get; } = DiscordColor.Orange;
        public static DiscordColor StatusColor { get; } = DiscordColor.DarkGray;
        public static DiscordColor PremiumColor { get; } = DiscordColor.Gold;

        public static string HappySmile { get; } = "(≧◡≦)";
        public static string SadSmile { get; } = "(⋟﹏⋞)";
    }
}
