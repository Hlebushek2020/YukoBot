using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using YukoBot.Commands.Attributes;
using YukoBot.Commands.Models;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;
using YukoBot.Settings;

namespace YukoBot.Commands
{
    [RequireRegistered]
    public class RegisteredUserCommandModule : CommandModule
    {
        public override string CommandAccessError => "Эта команда доступна для зарегистрированных пользователей!";

        public RegisteredUserCommandModule() : base(Categories.User) { }

        [Command("settings")]
        [Description("Данные для подключения.")]
        public async Task GetClientSettings(CommandContext ctx)
        {

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle($"{ctx.Member.DisplayName}");
            discordEmbed.AddField("Хост", Settings.ServerAddress);
            discordEmbed.AddField("Порт", Settings.ServerPort.ToString());
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("app")]
        [Description("Ссылка на скачивание актуальной версии клиента.")]
        public async Task GetClientApp(CommandContext ctx)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle($"{ctx.Member.DisplayName}")
                .WithDescription(YukoSettings.Current.ClientActualApp);
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("ban-reason")]
        [Aliases("reason")]
        [Description("Причина бана на текущем сервере")]
        public async Task BanReason(CommandContext ctx)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                 .WithTitle($"{ctx.Member.DisplayName}");

            YukoDbContext dbCtx = new YukoDbContext();
            List<DbBan> dbBanList = dbCtx.Bans.Where(x => x.ServerId == ctx.Guild.Id && x.UserId == ctx.Member.Id).ToList();
            if (dbBanList.Count > 0)
            {
                discordEmbed.WithColor(DiscordColor.Orange);

                DbBan dbBan = dbBanList[0];
                if (string.IsNullOrEmpty(dbBan.Reason))
                {
                    discordEmbed.WithDescription("К сожалению причина бана не была указана.");
                }
                else
                {
                    discordEmbed.WithDescription(dbBan.Reason);
                }
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Вы не забанены. (≧◡≦)");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("password-reset")]
        [Aliases("password")]
        [Description("Сброс пароля")]
        public async Task PasswordReset(CommandContext ctx)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                 .WithTitle($"{ctx.Member.DisplayName}");

            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = dbCtx.Users.Find(ctx.Member.Id);

            string password = "";
            Random random = new Random();
            while (password.Length != 10)
            {
                password += (char)random.Next(33, 127);
            }

            using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder hashBuilder = new StringBuilder(hashBytes.Length / 2);
                foreach (byte code in hashBytes)
                {
                    hashBuilder.Append(code.ToString("X2"));
                }
                dbUser.Password = hashBuilder.ToString();
            }

            await dbCtx.SaveChangesAsync();

            DiscordDmChannel userChat = await ctx.Member.CreateDmChannelAsync();
            DiscordEmbedBuilder discordEmbedDm = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle("Пароль сменен!");
            discordEmbedDm.AddField("Новый пароль", password);
            DiscordMessage userMessage = await userChat.SendMessageAsync(discordEmbedDm);
            await userMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, ":negative_squared_cross_mark:", false));

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Пароль сменен! Новый пароль отправлен в личные сообщения. (≧◡≦)");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("info-message-pm")]
        [Description("Отключает или включает отправку информационных сообщений в личные сообщения (работает для команды add)")]
        public async Task InfoMessagesInPM(CommandContext ctx,
            [Description("true - включить / false - отключить")] bool isEnabled)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(ctx.Member.DisplayName);

            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = dbCtx.Users.Find(ctx.Member.Id);

            if (dbUser.InfoMessages != isEnabled)
            {
                dbUser.InfoMessages = isEnabled;
                await dbCtx.SaveChangesAsync();
            }

            discordEmbed.WithColor(DiscordColor.Orange)
                .WithDescription($"{(isEnabled ? "Включено" : "Отключено")}! (≧◡≦)");

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("bug-report")]
        [Description("Позволяет сообщить об ошибке")]
        public async Task BugReport(CommandContext ctx,
            [Description("Описание ошибки"), RemainingText] string description)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                  .WithTitle(ctx.Member.DisplayName);

            if (Settings.BugReport)
            {
                DiscordMessage discordMessage = ctx.Message;

                Dictionary<string, Stream> files = new Dictionary<string, Stream>();
                foreach (DiscordAttachment attachment in discordMessage.Attachments)
                {
                    WebClient webClient = new WebClient();
                    MemoryStream memoryStream = new MemoryStream(webClient.DownloadData(attachment.Url));
                    files.Add(files.Count + attachment.FileName, memoryStream);
                }

                DiscordEmbedBuilder reportEmbed = new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Orange)
                    .WithTitle("Bug-Report")
                    .AddField("Author", ctx.User.Username + "#" + ctx.User.Discriminator)
                    .AddField("Guild", ctx.Guild.Name)
                    .AddField("Description", description)
                    .AddField("Date", discordMessage.CreationTimestamp.LocalDateTime.ToString("dd.MM.yyyy HH:mm:ss"));

                DiscordMessageBuilder reportMessage = new DiscordMessageBuilder()
                    .WithEmbed(reportEmbed)
                    .AddFiles(files, true);

                DiscordGuild reportGuild = await ctx.Client.GetGuildAsync(Settings.BugReportServer);
                DiscordChannel reportChannel = reportGuild.GetChannel(Settings.BugReportChannel);

                await reportChannel.SendMessageAsync(reportMessage);

                discordEmbed.WithColor(DiscordColor.Orange)
                    .WithDescription("Баг-репорт успешно отправлен! (≧◡≦)");
            }
            else
            {
                discordEmbed.WithColor(DiscordColor.Red)
                    .WithDescription("Ой, эта команда отключена (⋟﹏⋞)");
            }

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("profile")]
        [Aliases("me")]
        [Description("Показать информацию обо мне (только связанная с ботом)")]
        public async Task Profile(CommandContext ctx)
        {
            YukoDbContext dbContext = new YukoDbContext();
            DbUser dbUser = dbContext.Users.Find(ctx.User.Id);

            DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder()
                .WithThumbnail(ctx.User.AvatarUrl)
                .WithTitle(ctx.Member != null ? ctx.Member.DisplayName : ctx.User.Username)
                .AddField("Премиум: ", dbUser.HasPremium ? "есть" : "нет", true)
                .AddField("Последний вход в приложение: ", dbUser.LoginTime != null ? dbUser.LoginTime?.ToString("dd.MM.yyyy HH:mm") : "", true)
                .AddField("Необязательные уведомления: ", dbUser.InfoMessages ? "включены" : "отключены", true)
                .WithColor(dbUser.HasPremium ? DiscordColor.Gold : DiscordColor.Orange);

            IList<DbBan> bans = dbContext.Bans.Where(x => x.UserId == dbUser.Id).ToList();
            StringBuilder banListBuilder = new StringBuilder();
            foreach (DbBan ban in bans)
            {
                if (banListBuilder.Length > 0)
                {
                    banListBuilder.AppendLine();
                }
                DiscordGuild discordGuild = ctx.Client.Guilds[ban.ServerId];
                banListBuilder.Append(discordGuild.Name).Append(" - ").Append(string.IsNullOrEmpty(ban.Reason) ? "не указана" : ban.Reason);
            }

            embedBuilder.AddField("Список текущих банов:", banListBuilder.Length > 0 ? banListBuilder.ToString() : "Отсутствуют");

            await ctx.RespondAsync(embedBuilder);
        }
    }
}