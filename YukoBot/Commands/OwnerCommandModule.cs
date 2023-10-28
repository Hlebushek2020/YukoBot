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

        private readonly YukoDbContext _yukoDbContext;
        private readonly IYukoBot _yukoBot;
        private readonly IYukoSettings _yukoSettings;

        public OwnerCommandModule(YukoDbContext yukoDbContext, IYukoBot yukoBot, IYukoSettings yukoSettings)
            : base(Categories.Management, Resources.OwnerCommand_AccessError)
        {
            _yukoDbContext = yukoDbContext;
            _yukoBot = yukoBot;
            _yukoSettings = yukoSettings;
        }

        [Command("shutdown")]
        [Aliases("sd")]
        [Description("OwnerCommand.Shutdown")]
        public async Task Shutdown(
            CommandContext ctx,
            [Description("CommandArg.Reason"), RemainingText]
            string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                await ctx.RespondAsync(
                    string.Format(
                        Resources.OwnerCommand_Shutdown_ReasonIsEmpty,
                        Constants.SadSmile));
            }
            else
            {
                await ctx.RespondAsync(
                    string.Format(
                        Resources.OwnerCommand_Shutdown_Response,
                        Constants.HappySmile));
                _yukoBot.Shutdown(reason);
            }
        }

        [Command("status")]
        [Aliases("stat")]
        [Description("OwnerCommand.Status")]
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
                    timeSpan.Days,
                    timeSpan.Hours,
                    timeSpan.Minutes,
                    timeSpan.Seconds));

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("set-app")]
        [Description("OwnerCommand.SetApp")]
        public async Task SetApp(CommandContext ctx, string newlink)
        {
            _yukoSettings.SetApp(newlink);
            await ctx.RespondAsync(
                string.Format(
                    Resources.OwnerCommand_SetApp_Response,
                    Constants.HappySmile));
        }

        [Command("extend-premium")]
        [Aliases("ep")]
        [Description("OwnerCommand.ExtendPremium")]
        public async Task ExtendPremium(
            CommandContext ctx,
            [Description("CommandArg.Member")]
            DiscordMember discordMember,
            [Description("CommandArg.ExtendPremiumValue")]
            int count,
            [Description("CommandArg.ExtendPremiumValueType")]
            string type)
        {
            type = type.ToLower();
            if (!type.Equals(ExtendPremiumDayFull) && !type.Equals(ExtendPremiumDayShort) &&
                !type.Equals(ExtendPremiumMonthFull) && !type.Equals(ExtendPremiumMonthShort) &&
                !type.Equals(ExtendPremiumYearFull) && !type.Equals(ExtendPremiumYearShort))
            {
                await ctx.RespondAsync(
                    string.Format(
                        Resources.OwnerCommand_ExtendPremium_IncorrectUnit,
                        Constants.SadSmile));
            }
            else
            {
                DbUser dbUser = await _yukoDbContext.Users.FindAsync(discordMember.Id);
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
                    await _yukoDbContext.SaveChangesAsync();

                    await ctx.RespondAsync(
                        string.Format(
                            Resources.OwnerCommand_ExtendPremium_Response,
                            discordMember.DisplayName,
                            Constants.HappySmile));
                }
                else
                {
                    await ctx.RespondAsync(
                        string.Format(
                            Resources.OwnerCommand_ExtendPremium_MemberNotRegistered,
                            Constants.SadSmile));
                }
            }
        }
    }
}