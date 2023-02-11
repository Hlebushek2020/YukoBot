using System;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using YukoBot.Commands.Attributes;
using YukoBot.Commands.Models;
using YukoBot.Extensions;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;
using YukoBot.Settings;

namespace YukoBot.Commands
{
    [RequireRegistered]
    public class RegisteredUserCommandModule : CommandModule
    {
        public override string CommandAccessError =>
            "Простите, эта команда доступна для зарегистрированных пользователей!";

        public RegisteredUserCommandModule() : base(Categories.User)
        {
        }

        [Command("settings")]
        [Description("Показать настройки для подключения к боту.")]
        public async Task GetClientSettings(CommandContext ctx)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyTitle(ctx.Member.DisplayName)
                .WithColor(Constants.SuccessColor)
                .AddField("Хост", Settings.ServerAddress)
                .AddField("Порт", Settings.ServerPort.ToString());

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("app")]
        [Description("Показать ссылку на скачивание актуальной версии клиента.")]
        public async Task GetClientApp(CommandContext ctx)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(ctx.Member.DisplayName, YukoSettings.Current.ClientActualApp);

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("ban-reason")]
        [Aliases("reason")]
        [Description("Причина бана на текущем сервере.")]
        public async Task BanReason(CommandContext ctx)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(Constants.SuccessColor);

            YukoDbContext dbCtx = new YukoDbContext();
            List<DbBan> dbBanList = dbCtx.Bans.Where(x => x.ServerId == ctx.Guild.Id && x.UserId == ctx.Member.Id)
                .ToList();
            if (dbBanList.Count > 0)
            {
                DbBan dbBan = dbBanList[0];
                discordEmbed.WithSadTitle(ctx.Member.DisplayName);
                if (string.IsNullOrEmpty(dbBan.Reason))
                {
                    discordEmbed.WithDescription("К сожалению причина бана не была указана.");
                }
                else
                {
                    discordEmbed.WithDescription(dbBan.Reason);
                }
            }
            else
            {
                discordEmbed.WithHappyTitle(ctx.Member.DisplayName)
                    .WithDescription("Вы не забанены!");
            }

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("info-message-pm")]
        [Description(
            "Отправка сообщения об успешности выполнения команды `add` в ЛС (работает если сообщения об успешности выполнения команды `add` отключены на сервере)")]
        public async Task InfoMessagesInPM(CommandContext ctx,
            [Description("true - включить / false - отключить")]
            bool isEnabled)
        {
            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = dbCtx.Users.Find(ctx.Member.Id);

            if (dbUser.InfoMessages != isEnabled)
            {
                dbUser.InfoMessages = isEnabled;
                await dbCtx.SaveChangesAsync();
            }

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(ctx.Member.DisplayName, isEnabled ? "Включено!" : "Отключено!");

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("bug-report")]
        [Description("Сообщить об ошибке.")]
        public async Task BugReport(CommandContext ctx,
            [Description("Описание ошибки"), RemainingText]
            string description)
        {
            DiscordEmbedBuilder discordEmbed = null;

            if (Settings.BugReport)
            {
                EventId eventId = new EventId(0, $"Command: {ctx.Command.Name}");
                DiscordMessage discordMessage = ctx.Message;

                DiscordEmbedBuilder reportEmbed = new DiscordEmbedBuilder()
                    .WithColor(Constants.SuccessColor)
                    .WithTitle("Bug-Report")
                    .AddField("Author", ctx.User.Username + "#" + ctx.User.Discriminator)
                    .AddField("Guild", ctx.Guild.Name)
                    .AddField("Date", discordMessage.CreationTimestamp.LocalDateTime.ToString("dd.MM.yyyy HH:mm:ss"));

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
                        _defaultLogger.LogWarning(eventId, ex, "");
                    }
                }

                DiscordMessage referencedMessage = discordMessage.ReferencedMessage;

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

                DiscordGuild reportGuild = await ctx.Client.GetGuildAsync(Settings.BugReportServer);
                DiscordChannel reportChannel = reportGuild.GetChannel(Settings.BugReportChannel);

                await reportChannel.SendMessageAsync(reportMessage);

                discordEmbed = new DiscordEmbedBuilder()
                    .WithHappyMessage(ctx.Member.DisplayName, "Баг-репорт успешно отправлен!");
            }
            else
            {
                discordEmbed = new DiscordEmbedBuilder()
                    .WithSadMessage(ctx.Member.DisplayName, "Ой, эта команда отключена!");
            }

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("profile")]
        [Aliases("me")]
        [Description("Показать информацию о моей учетной записи бота.")]
        public async Task Profile(CommandContext ctx)
        {
            YukoDbContext dbContext = new YukoDbContext();
            DbUser dbUser = dbContext.Users.Find(ctx.User.Id);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithHappyTitle(ctx.Member != null ? ctx.Member.DisplayName : ctx.User.Username)
                .WithThumbnail(ctx.User.AvatarUrl)
                .AddField("Премиум: ", dbUser.HasPremium ? "Есть" : "Нет", true)
                .AddField("Последний вход в приложение: ",
                    dbUser.LoginTime != null ? dbUser.LoginTime?.ToString("dd.MM.yyyy HH:mm") : "-", true)
                .AddField("Необязательные уведомления: ", dbUser.InfoMessages ? "Включены" : "Отключены", true)
                .WithColor(dbUser.HasPremium ? Constants.PremiumColor : Constants.SuccessColor);

            IList<DbBan> bans = dbContext.Bans.Where(x => x.UserId == dbUser.Id).ToList();
            StringBuilder banListBuilder = new StringBuilder();
            foreach (DbBan ban in bans)
            {
                if (banListBuilder.Length > 0)
                {
                    banListBuilder.AppendLine();
                }
                DiscordGuild discordGuild = ctx.Client.Guilds[ban.ServerId];
                banListBuilder.Append(discordGuild.Name).Append(" - ")
                    .Append(string.IsNullOrEmpty(ban.Reason) ? "не указана" : ban.Reason);
            }
            embedBuilder.AddField("Список текущих банов:",
                banListBuilder.Length > 0 ? banListBuilder.ToString() : "Отсутствуют");
            await ctx.RespondAsync(embedBuilder);
        }
    }
}