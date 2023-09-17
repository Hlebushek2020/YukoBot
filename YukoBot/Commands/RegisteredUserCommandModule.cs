using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using YukoBot.Commands.Attributes;
using YukoBot.Commands.Models;
using YukoBot.Extensions;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Commands
{
    [RequireRegistered]
    public class RegisteredUserCommandModule : CommandModule
    {
        #region Constants
        private const string ProfileDtf = "d MMMM yyyy г. hh:mm";
        #endregion

        private readonly YukoDbContext _dbContext;
        private readonly IYukoSettings _yukoSettings;
        private readonly ILogger<RegisteredUserCommandModule> _logger;

        public RegisteredUserCommandModule(
            YukoDbContext dbContext,
            IYukoSettings yukoSettings,
            ILogger<RegisteredUserCommandModule> logger)
            : base(Categories.User, Resources.RegisteredUserCommand_AccessError)
        {
            _dbContext = dbContext;
            _yukoSettings = yukoSettings;
            _logger = logger;
        }

        [Command("settings")]
        [Description("RegisteredUserCommand.Settings")]
        public async Task Settings(CommandContext ctx)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyTitle(ctx.Member.DisplayName)
                .WithColor(Constants.SuccessColor)
                .AddField(Resources.RegisteredUserCommand_Settings_FieldHost_Title, _yukoSettings.ServerAddress)
                .AddField(
                    Resources.RegisteredUserCommand_Settings_FieldPort_Title,
                    _yukoSettings.ServerPort.ToString());

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("app")]
        [Description("RegisteredUserCommand.GetClientApp")]
        public async Task GetClientApp(CommandContext ctx)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(ctx.Member.DisplayName, _yukoSettings.ClientActualApp);

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("ban-reason")]
        [Aliases("reason")]
        [Description("RegisteredUserCommand.BanReason")]
        public async Task BanReason(CommandContext ctx)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(Constants.SuccessColor);

            List<DbBan> dbBanList = _dbContext.Bans
                .Where(x => x.ServerId == ctx.Guild.Id && x.UserId == ctx.Member.Id).ToList();
            if (dbBanList.Count > 0)
            {
                DbBan dbBan = dbBanList[0];
                discordEmbed.WithSadTitle(ctx.Member.DisplayName);
                discordEmbed.WithDescription(
                    string.IsNullOrEmpty(dbBan.Reason)
                        ? Resources.RegisteredUserCommand_BanReason_NotBanReason
                        : dbBan.Reason);
            }
            else
            {
                discordEmbed.WithHappyTitle(ctx.Member.DisplayName)
                    .WithDescription(Resources.RegisteredUserCommand_BanReason_NotBan);
            }

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("info-message-pm")]
        [Description("RegisteredUserCommand.InfoMessagesInPM")]
        public async Task InfoMessagesInPm(
            CommandContext ctx,
            [Description("true - включить / false - отключить")]
            bool isEnabled)
        {
            DbUser dbUser = await _dbContext.Users.FindAsync(ctx.Member.Id);

            if (dbUser.InfoMessages != isEnabled)
            {
                dbUser.InfoMessages = isEnabled;
                await _dbContext.SaveChangesAsync();
            }

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(
                    ctx.Member.DisplayName,
                    isEnabled
                        ? Resources.RegisteredUserCommand_InfoMessagesInPM_Description_Enabled
                        : Resources.RegisteredUserCommand_InfoMessagesInPM_Description_Disabled);

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("bug-report")]
        [Description("Сообщить об ошибке.")]
        public async Task BugReport(
            CommandContext ctx,
            [Description("RegisteredUserCommand.BugReport"), RemainingText]
            string description)
        {
            DiscordEmbedBuilder discordEmbed = null;

            if (_yukoSettings.BugReport)
            {
                DiscordMessage discordMessage = ctx.Message;
                DiscordMessage referencedMessage = discordMessage.ReferencedMessage;

                if (referencedMessage == null && discordMessage.Attachments.Count == 0 &&
                    string.IsNullOrEmpty(description))
                {
                    discordEmbed = new DiscordEmbedBuilder()
                        .WithSadMessage(
                            ctx.Member.DisplayName,
                            Resources.RegisteredUserCommand_BugReport_Description_IsEmpty);
                }
                else
                {
                    DiscordEmbedBuilder reportEmbed = new DiscordEmbedBuilder()
                        .WithColor(Constants.SuccessColor)
                        .WithTitle("Bug-Report")
                        .AddField("Author", ctx.User.Username)
                        .AddField("Guild", ctx.Guild.Name)
                        .AddField(
                            "Date",
                            discordMessage.CreationTimestamp.LocalDateTime.ToString("dd.MM.yyyy HH:mm:ss"));

                    if (!string.IsNullOrEmpty(description))
                    {
                        reportEmbed.AddField("Description", description);
                    }

                    DiscordMessageBuilder reportMessage = new DiscordMessageBuilder().WithEmbed(reportEmbed);
                    foreach (DiscordAttachment attachment in discordMessage.Attachments)
                    {
                        try
                        {
                            using HttpClient client = new HttpClient();
                            Stream fileStream = await client.GetStreamAsync(attachment.Url);
                            string fileName = attachment.FileName ?? Guid.NewGuid().ToString();
                            reportMessage.AddFile(fileName, fileStream);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning($"Failed to download attachment. Exception: {ex.Message}.");
                        }
                    }

                    if (referencedMessage != null)
                    {
                        if (!string.IsNullOrEmpty(referencedMessage.Content))
                        {
                            reportEmbed.AddField("Reference Message", referencedMessage.Content);
                        }

                        if (referencedMessage.Embeds != null)
                        {
                            reportMessage.AddEmbeds(referencedMessage.Embeds);
                        }
                    }

                    DiscordGuild reportGuild = await ctx.Client.GetGuildAsync(_yukoSettings.BugReportServer);
                    DiscordChannel reportChannel = reportGuild.GetChannel(_yukoSettings.BugReportChannel);

                    await reportChannel.SendMessageAsync(reportMessage);

                    discordEmbed = new DiscordEmbedBuilder()
                        .WithHappyMessage(
                            ctx.Member.DisplayName,
                            Resources.RegisteredUserCommand_BugReport_Description_IsSuccess);
                }
            }
            else
            {
                discordEmbed = new DiscordEmbedBuilder()
                    .WithSadMessage(
                        ctx.Member.DisplayName,
                        Resources.RegisteredUserCommand_BugReport_Description_IsDisabled);
            }

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("profile")]
        [Aliases("me")]
        [Description("RegisteredUserCommand.Profile")]
        public async Task Profile(CommandContext ctx)
        {
            // TODO: ??????????????????????
            CultureInfo locale = CultureInfo.GetCultureInfo("ru-RU");

            DbUser dbUser = await _dbContext.Users.FindAsync(ctx.User.Id);
            bool hasPremiumAccess = dbUser.HasPremiumAccess;

            string fieldPremiumText = dbUser.PremiumAccessExpires.HasValue
                ? hasPremiumAccess
                    ? Resources.RegisteredUserCommand_Profile_FieldPremium_Expires
                    : Resources.RegisteredUserCommand_Profile_FieldPremium_Expired
                : Resources.RegisteredUserCommand_Profile_FieldPremium_NotSet;

            if (dbUser.PremiumAccessExpires.HasValue)
            {
                fieldPremiumText = string.Format(
                    fieldPremiumText,
                    dbUser.PremiumAccessExpires.Value.ToString(ProfileDtf, locale));
            }

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithHappyTitle(ctx.Member != null ? ctx.Member.DisplayName : ctx.User.Username)
                .WithThumbnail(ctx.User.AvatarUrl)
                .AddField(Resources.RegisteredUserCommand_Profile_FieldPremium_Title, fieldPremiumText, true)
                .AddField(
                    Resources.RegisteredUserCommand_Profile_FieldLastLogin_Title,
                    dbUser.LoginTime.HasValue ? dbUser.LoginTime.Value.ToString(ProfileDtf, locale) : "-",
                    true)
                .AddField(
                    Resources.RegisteredUserCommand_Profile_FieldOptionalNotifications_Title,
                    dbUser.InfoMessages
                        ? Resources.RegisteredUserCommand_Profile_FieldOptionalNotifications_Enabled
                        : Resources.RegisteredUserCommand_Profile_FieldOptionalNotifications_Disabled,
                    true)
                .WithColor(hasPremiumAccess ? Constants.PremiumAccessColor : Constants.SuccessColor);

            IList<DbBan> bans = _dbContext.Bans.Where(x => x.UserId == dbUser.Id).ToList();
            StringBuilder banListBuilder = new StringBuilder();
            foreach (DbBan ban in bans)
            {
                if (banListBuilder.Length > 0)
                {
                    banListBuilder.AppendLine();
                }
                DiscordGuild discordGuild = ctx.Client.Guilds[ban.ServerId];
                banListBuilder.Append(discordGuild.Name)
                    .Append(" - ")
                    .Append(
                        string.IsNullOrEmpty(ban.Reason)
                            ? Resources.RegisteredUserCommand_Profile_FieldBanList_ReasonNotSpecified
                            : ban.Reason);
            }
            embedBuilder.AddField(
                Resources.RegisteredUserCommand_Profile_FieldBanList_Title,
                banListBuilder.Length > 0
                    ? banListBuilder.ToString()
                    : Resources.RegisteredUserCommand_Profile_FieldBanList_IsEmpty);

            await ctx.RespondAsync(embedBuilder);
        }
    }
}