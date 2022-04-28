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

namespace YukoBot.Commands
{
    [RequireRegistered]
    public class RegisteredUserCommandModule : CommandModule
    {
        public RegisteredUserCommandModule() : base(Category.User)
        {
            CommandAccessError = "Эта команда доступна для зарегистрированных пользователей!";
        }

        [Command("settings")]
        [Description("Данные для подключения.")]
        public async Task GetClientSettings(CommandContext commandContext)
        {
            YukoSettings settings = YukoSettings.Current;

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle($"{commandContext.Member.DisplayName}");
            discordEmbed.AddField("Хост", settings.ServerAddress);
            discordEmbed.AddField("Порт", settings.ServerPort.ToString());
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("app")]
        [Description("Ссылка на скачивание актуальной версии клиента.")]
        public async Task GetClientApp(CommandContext commandContext)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle($"{commandContext.Member.DisplayName}")
                .WithDescription(YukoSettings.Current.ClientActualApp);
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("ban-reason")]
        [Aliases("reason")]
        [Description("Причина бана на текущем сервере")]
        public async Task BanReason(CommandContext commandContext)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                 .WithTitle($"{commandContext.Member.DisplayName}");

            YukoDbContext dbContext = new YukoDbContext();
            List<DbBan> currentBans = dbContext.Bans.Where(x => x.ServerId == commandContext.Guild.Id && x.UserId == commandContext.Member.Id).ToList();
            if (currentBans.Count > 0)
            {
                discordEmbed.WithColor(DiscordColor.Orange);

                DbBan ban = currentBans[0];
                if (string.IsNullOrEmpty(ban.Reason))
                {
                    discordEmbed.WithDescription("К сожалению причина бана не была указана.");
                }
                else
                {
                    discordEmbed.WithDescription(ban.Reason);
                }
                await commandContext.RespondAsync(discordEmbed);
                return;
            }

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Вы не забанены. (≧◡≦)");
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("password-reset")]
        [Aliases("password")]
        [Description("Сброс пароля")]
        public async Task PasswordReset(CommandContext commandContext)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                 .WithTitle($"{commandContext.Member.DisplayName}");

            YukoDbContext dbContext = new YukoDbContext();
            DbUser dbUser = dbContext.Users.Find(commandContext.Member.Id);

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

            await dbContext.SaveChangesAsync();

            DiscordDmChannel userChat = await commandContext.Member.CreateDmChannelAsync();
            DiscordEmbedBuilder discordEmbedDm = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle("Пароль сменен!");
            discordEmbedDm.AddField("Новый пароль", password);
            await userChat.SendMessageAsync(discordEmbedDm);

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Пароль сменен! Новый пароль отправлен в личные сообщения. (≧◡≦)");
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("info-message-pm")]
        [Description("Отключает или включает отправку информационных сообщений в личные сообщения (работает для команды add)")]
        public async Task InfoMessagesInPM(CommandContext commandContext,
            [Description("true - включить / false - отключить")] bool isEnabled)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(commandContext.Member.DisplayName);

            YukoDbContext dbContext = new YukoDbContext();
            DbUser dbUser = dbContext.Users.Find(commandContext.Member.Id);

            if (dbUser.InfoMessages != isEnabled)
            {
                dbUser.InfoMessages = isEnabled;
                await dbContext.SaveChangesAsync();
            }

            discordEmbed.WithColor(DiscordColor.Orange)
                .WithDescription($"{(isEnabled ? "Включено" : "Отключено")}! (≧◡≦)");

            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("bug-report")]
        [Description("Позволяет сообщить об ошибке")]
        public async Task BugReport(CommandContext commandContext,
            [Description("Описание ошибки"), RemainingText] string description)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                  .WithTitle(commandContext.Member.DisplayName);

            YukoSettings settings = YukoSettings.Current;
            if (settings.BugReport)
            {
                DiscordMessage discordMessage = commandContext.Message;

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
                    .AddField("Author", commandContext.User.Username + "#" + commandContext.User.Discriminator)
                    .AddField("Description", description)
                    .AddField("Date", discordMessage.CreationTimestamp.LocalDateTime.ToString("dd.MM.yyyy HH:mm:ss"));

                DiscordMessageBuilder reportMessage = new DiscordMessageBuilder()
                    .WithEmbed(reportEmbed)
                    .WithFiles(files);

                DiscordGuild reportGuild = await commandContext.Client.GetGuildAsync(settings.BugReportServer);
                DiscordChannel reportChannel = reportGuild.GetChannel(settings.BugReportChannel);

                await reportChannel.SendMessageAsync(reportMessage);

                discordEmbed.WithColor(DiscordColor.Orange)
                    .WithDescription("Баг-репорт успешно отправлен! (≧◡≦)");
            }
            else
            {
                discordEmbed.WithColor(DiscordColor.Red)
                    .WithDescription("Ой, эта команда отключена (⋟﹏⋞)");
            }

            await commandContext.RespondAsync(discordEmbed);
        }
    }
}