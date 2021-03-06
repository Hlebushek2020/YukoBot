using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YukoBot.Commands.Attributes;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Commands
{
    [RequireOwnerAndUserPermissions(Permissions.Administrator)]
    public class AdminCommandModule : CommandModule
    {
        public AdminCommandModule() : base(Models.Category.Management)
        {
            CommandAccessError = "Эта команда доступна админу гильдии (сервера) и владельцу бота!";
        }

        [Command("ban")]
        [Description("Запрещает пользователю скачивать с этого сервера (гильдии)")]
        public async Task Ban(CommandContext commandContext,
            [Description("Участник сервера (гильдии)")] DiscordMember discordMember,
            [Description("Причина (Необязательно)"), RemainingText] string reason = null)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{commandContext.Member.DisplayName}");

            YukoDbContext dbContext = new YukoDbContext();
            DbUser dbUser = dbContext.Users.Find(discordMember.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Невозможно забанить незарегистрированного участника!");
                await commandContext.RespondAsync(discordEmbed);
                return;
            }

            int isBanned = dbContext.Bans.Where(x => x.ServerId == discordMember.Guild.Id && x.UserId == discordMember.Id).Count();
            if (isBanned > 0)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Участник уже забанен. (≧◡≦)");
                await commandContext.RespondAsync(discordEmbed);
                return;
            }

            DbBan dbBan = new DbBan
            {
                User = dbUser,
                ServerId = discordMember.Guild.Id,
                Reason = reason
            };

            dbContext.Bans.Add(dbBan);
            await dbContext.SaveChangesAsync();

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Участник успешно забанен!");
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("unban")]
        [Description("Удаляет пользователя из забаненых (пользователю снова разрешено скачивать с этого сервера (гильдии))")]
        public async Task UnBan(CommandContext commandContext,
            [Description("Участник сервера (гильдии)")] DiscordMember discordMember)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{commandContext.Member.DisplayName}");

            YukoDbContext dbContext = new YukoDbContext();
            DbUser dbUser = dbContext.Users.Find(discordMember.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Невозможно разбанить незарегистрированного участника!");
                await commandContext.RespondAsync(discordEmbed);
                return;
            }

            List<DbBan> currentBans = dbContext.Bans.Where(x => x.ServerId == discordMember.Guild.Id && x.UserId == discordMember.Id).ToList();
            if (currentBans.Count > 0)
            {
                foreach (DbBan dbBan in currentBans)
                {
                    dbContext.Bans.Remove(dbBan);
                }
                await dbContext.SaveChangesAsync();
            }

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Участник успешно разбанен. (≧◡≦)");
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("member-ban-reason")]
        [Aliases("m-reason")]
        [Description("Причина бана участника сервера")]
        public async Task MemberBanReason(CommandContext commandContext,
            [Description("Участник сервера (гильдии)")] DiscordMember discordMember)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                 .WithTitle($"{commandContext.Member.DisplayName}");

            YukoDbContext dbContext = new YukoDbContext();
            DbUser dbUser = dbContext.Users.Find(discordMember.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription($"Участник {discordMember.DisplayName} не зарегистрирован!");
                await commandContext.RespondAsync(discordEmbed);
                return;
            }

            List<DbBan> currentBans = dbContext.Bans.Where(x => x.ServerId == discordMember.Guild.Id && x.UserId == discordMember.Id).ToList();
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
                .WithDescription($"Участник {discordMember.DisplayName} не забанен. (≧◡≦)");
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("set-art-channel")]
        [Description("Устанавливает канал для поиска сообщений при использовании комманд категории \"Управление коллекциями\"")]
        public async Task SetArtChannel(CommandContext commandContext,
            [Description("Канал для поиска сообщений")] DiscordChannel discordChannel)
        {
            YukoDbContext dbContext = new YukoDbContext();
            DbGuildSettings guildArtChannel = dbContext.GuildsSettings.Find(commandContext.Guild.Id);
            if (guildArtChannel != null)
            {
                guildArtChannel.ArtChannelId = discordChannel.Id;
            }
            else
            {
                dbContext.GuildsSettings.Add(new DbGuildSettings
                {
                    Id = commandContext.Guild.Id,
                    ArtChannelId = discordChannel.Id
                });
            }
            await dbContext.SaveChangesAsync();
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
               .WithTitle($"{commandContext.Member.DisplayName}")
               .WithColor(DiscordColor.Orange)
               .WithDescription("Канал успешно установлен! (≧◡≦)");
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("add-command-response")]
        [Aliases("add-response")]
        [Description("Отключает сообщение об успешности выполнения команды add на сервере, взамен сообщение будет приходить в ЛС")]
        public async Task AddCommandResponse(CommandContext commandContext,
            [Description("true - включить / false - отключить")] bool isEnabled)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(commandContext.Member.DisplayName);

            YukoDbContext dbContext = new YukoDbContext();
            DbGuildSettings guildArtChannel = dbContext.GuildsSettings.Find(commandContext.Guild.Id);
            if (guildArtChannel != null)
            {
                guildArtChannel.AddCommandResponse = isEnabled;
            }
            else
            {
                dbContext.GuildsSettings.Add(new DbGuildSettings
                {
                    Id = commandContext.Guild.Id,
                    AddCommandResponse = isEnabled
                });
            }
            await dbContext.SaveChangesAsync();
            discordEmbed.WithColor(DiscordColor.Orange)
                .WithDescription($"{(isEnabled ? "Включено" : "Отключено")}! (≧◡≦)");

            await commandContext.RespondAsync(discordEmbed);
        }
    }
}