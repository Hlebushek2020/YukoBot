using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using YukoBot.Commands.Models;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

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

        private readonly IYukoBot _yukoBot;
        private readonly IYukoSettings _yukoSettings;

        public OwnerCommandModule(IYukoBot yukoBot, IYukoSettings yukoSettings)
            : base(Categories.Management, Resources.OwnerCommand_AccessError)
        {
            _yukoBot = yukoBot;
            _yukoSettings = yukoSettings;
        }

        [Command("shutdown")]
        [Aliases("sd")]
        [Description("Выключить бота.")]
        public async Task Shutdown(
            CommandContext ctx,
            [Description("Причина выключения бота"), RemainingText]
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                await ctx.RespondAsync(string.Format(
                    Resources.OwnerCommand_Shutdown_ReasonIsEmpty, Constants.SadSmile));
            }
            else
            {
                await ctx.RespondAsync(string.Format(
                    Resources.OwnerCommand_Shutdown_Response, Constants.HappySmile));
                _yukoBot.Shutdown(reason);
            }
        }

        [Command("status")]
        [Aliases("stat")]
        [Description("Сведения о боте.")]
        public async Task Status(CommandContext ctx)
        {
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(Constants.StatusColor)
                .AddField("Net", $"v{Environment.Version}")
                .AddField("DSharpPlus", $"v{ctx.Client.VersionString}")
                .AddField(
                    Resources.OwnerCommand_Status_FieldAssembly_Title,
                    $"v{Program.Version} {File.GetCreationTime(assemblyLocation):dd.MM.yyyy}")
                .AddField(
                    Resources.OwnerCommand_Status_FieldLaunchDate_Title,
                    _yukoBot.StartDateTime.ToString("dd.MM.yyyy HH:mm:ss zzz"));

            TimeSpan timeSpan = DateTime.Now - _yukoBot.StartDateTime;
            discordEmbed.AddField(
                Resources.OwnerCommand_Status_FieldWorkingHours_Title,
                string.Format(
                    Resources.OwnerCommand_Status_FieldWorkingHours_Description,
                    timeSpan.Days, timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds));

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("set-app")]
        [Description("Установить новую ссылку для команды `app`.")]
        public async Task SetApp(CommandContext ctx, string newlink)
        {
            _yukoSettings.SetApp(newlink);
            await ctx.RespondAsync(string.Format(
                Resources.OwnerCommand_SetApp_Response, Constants.HappySmile));
        }

        [Command("extend-premium")]
        [Aliases("ep")]
        [Description("Продлить премиум доступ.")]
        public async Task ExtendPremium(
            CommandContext ctx,
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
                await ctx.RespondAsync(string.Format(
                    Resources.OwnerCommand_ExtendPremium_IncorrectUnit, Constants.SadSmile));
            }
            else
            {
                YukoDbContext dbCtx = new YukoDbContext(_yukoSettings);
                DbUser dbUser = await dbCtx.Users.FindAsync(discordMember.Id);
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
                        string.Format(
                            Resources.OwnerCommand_ExtendPremium_Response,
                            discordMember.DisplayName,
                            Constants.HappySmile));
                }
                else
                {
                    await ctx.RespondAsync(string.Format(
                        Resources.OwnerCommand_ExtendPremium_MemberNotRegistered, Constants.SadSmile));
                }
            }
        }
    }
}