using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using YukoBot.Commands.Models;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;
using YukoBot.Settings;

namespace YukoBot.Commands
{
    [RequireOwner]
    public class OwnerCommandModule : CommandModule
    {
        #region Constants
        private const string ExtendPremiumDayFull = "day";
        private const string ExtendPremiumDayShort = "d";
        private const string ExtendPremiumMonthFull = "month";
        private const string ExtendPremiumMonthShort = "m";
        private const string ExtendPremiumYearFull = "year";
        private const string ExtendPremiumYearShort = "y";
        #endregion

        public override string CommandAccessError => "Простите, эта команда доступна только владельцу бота!";

        public OwnerCommandModule() : base(Categories.Management)
        {
        }

        [Command("shutdown")]
        [Aliases("sd")]
        [Description("Выключить бота.")]
        public async Task Shutdown(CommandContext ctx)
        {
            await ctx.RespondAsync($"Хорошо, хозяин! {Constants.HappySmile}");
            YukoBot.Current.Shutdown();
        }

        [Command("status")]
        [Aliases("stat")]
        [Description("Сведения о боте.")]
        public async Task Status(CommandContext ctx)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(Constants.StatusColor)
                .AddField("Net", $"v{Environment.Version}")
                .AddField("Сборка",
                    $"v{version.Major}.{version.Minor}.{version.Build} {File.GetCreationTime(Assembly.GetExecutingAssembly().Location):dd.MM.yyyy}")
                .AddField("Дата запуска",
                    $"{YukoBot.Current.StartDateTime:dd.MM.yyyy} {YukoBot.Current.StartDateTime:HH:mm:ss zzz}");

            TimeSpan timeSpan = DateTime.Now - YukoBot.Current.StartDateTime;
            discordEmbed.AddField("Время работы",
                $"{timeSpan.Days}d, {timeSpan.Hours}h, {timeSpan.Minutes}m, {timeSpan.Seconds}s");

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("set-app")]
        [Description("Установить новую ссылку для команды `app`.")]
        public async Task SetApp(CommandContext ctx, string newlink)
        {
            YukoSettings.Current.SetApp(newlink);
            await ctx.RespondAsync($"Хозяин! Ссылка на приложение установлена! {Constants.HappySmile}");
        }

        [Command("extend-premium")]
        [Aliases("ep")]
        [Description("Продлить премиум доступ.")]
        public async Task ExtendPremium(CommandContext ctx,
            [Description("Участник сервера (гильдии).")]
            DiscordMember discordMember,
            [Description(
                "Значение, на которое нужно продлить премиум доступ. Если премиум доступ нужно уменьшить, то вводится отрицательное значение.")]
            int count,
            [Description(
                "Единица измерения для значения. Возможные значения: day / d - день; month / m - месяц; year / y - год.")]
            string type)
        {
            type = type.ToLower();
            if (!type.Equals(ExtendPremiumDayFull) && !type.Equals(ExtendPremiumDayShort) &&
                !type.Equals(ExtendPremiumMonthFull) && !type.Equals(ExtendPremiumMonthShort) &&
                !type.Equals(ExtendPremiumYearFull) && !type.Equals(ExtendPremiumYearShort))
            {
                await ctx.RespondAsync(
                    $"Хозяин! Пожалуйста, укажите единицу измерения для значения! {Constants.SadSmile}");
            }
            else
            {
                YukoDbContext dbCtx = new YukoDbContext();
                DbUser dbUser = dbCtx.Users.Find(discordMember.Id);
                if (dbUser != null)
                {
                    DateTime forAdding = DateTime.Now;
                    if (dbUser.PremiumAccessExpires != null && dbUser.PremiumAccessExpires.Value > forAdding)
                        forAdding = dbUser.PremiumAccessExpires.Value;

                    switch (type)
                    {
                        case ExtendPremiumDayFull:
                        case ExtendPremiumDayShort:
                            forAdding = forAdding.AddDays(count);
                            break;
                        case ExtendPremiumMonthFull:
                        case ExtendPremiumMonthShort:
                            forAdding = forAdding.AddMonths(count);
                            break;
                        default:
                            forAdding = forAdding.AddYears(count);
                            break;
                    }
                    dbUser.PremiumAccessExpires = forAdding;
                    await dbCtx.SaveChangesAsync();

                    await ctx.RespondAsync(
                        $"Хозяин! Премиум доступ для {discordMember.DisplayName} успешно продлен! {Constants.HappySmile}");
                }
                else
                {
                    await ctx.RespondAsync($"Хозяин! Данный участник сервера не зарегистрирован! {Constants.SadSmile}");
                }
            }
        }
    }
}