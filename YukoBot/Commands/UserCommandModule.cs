using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Commands
{
    public class UserCommandModule : CommandModule
    {
        public UserCommandModule()
        {
            ModuleName = "Пользовательские команды";
        }

        [Command("register")]
        [Aliases("reg")]
        [Description("Регистрация")]
        public async Task Register(CommandContext commandContext)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{commandContext.Member.DisplayName}");

            DiscordMember user = commandContext.Member;

            YukoDbContext database = new YukoDbContext();
            DbUser dbUser = database.Users.Find(user.Id);
            if (dbUser != null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Вы уже зарегистрированы!");
                await commandContext.RespondAsync(discordEmbed);
                return;
            }
            dbUser = new DbUser
            {
                Id = user.Id,
                Nikname = user.Username + "#" + user.Discriminator
            };

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

            database.Users.Add(dbUser);
            await database.SaveChangesAsync();

            DiscordDmChannel userChat = await user.CreateDmChannelAsync();
            DiscordEmbedBuilder discordEmbedDm = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle("Регистрация прошла успешно!");
            discordEmbedDm.AddField("Логин", $"Используй **{dbUser.Nikname}** или **{dbUser.Id}**");
            discordEmbedDm.AddField("Пароль", password);
            await userChat.SendMessageAsync(discordEmbedDm);

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Регистрация прошла успешно! Пароль для входа отправлен в личные сообщения. ≧◡≦");
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("ban-reason")]
        [Aliases("reason")]
        [Description("Причина бана на текущем сервере")]
        public async Task BanReason(CommandContext commandContext)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                 .WithTitle($"{commandContext.Member.DisplayName}");

            YukoDbContext database = new YukoDbContext();
            DbUser dbUser = database.Users.Find(commandContext.Member.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Вы не зарегистрированы!");
                await commandContext.RespondAsync(discordEmbed);
                return;
            }

            List<DbBan> currentBans = database.Bans.Where(x => x.ServerId == commandContext.Guild.Id && x.UserId == commandContext.Member.Id).ToList();
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
                .WithDescription("Вы не забанены. ≧◡≦");
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("password-reset")]
        [Aliases("password")]
        [Description("Сброс пароля")]
        public async Task PasswordReset(CommandContext commandContext)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                 .WithTitle($"{commandContext.Member.DisplayName}");

            YukoDbContext database = new YukoDbContext();
            DbUser dbUser = database.Users.Find(commandContext.Member.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Вы не зарегистрированы!");
                await commandContext.RespondAsync(discordEmbed);
                return;
            }

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

            await database.SaveChangesAsync();

            DiscordDmChannel userChat = await commandContext.Member.CreateDmChannelAsync();
            DiscordEmbedBuilder discordEmbedDm = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle("Пароль сменен!");
            discordEmbedDm.AddField("Новый пароль", password);
            await userChat.SendMessageAsync(discordEmbedDm);

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Пароль сменен! Новый пароль отправлен в личные сообщения. ≧◡≦");
            await commandContext.RespondAsync(discordEmbed);
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
    }
}