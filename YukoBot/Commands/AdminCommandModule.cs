using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YukoBot.Commands.Attributes;
using YukoBot.Commands.Models;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Commands
{
    [RequireOwnerAndUserPermissions(Permissions.Administrator)]
    public class AdminCommandModule : CommandModule
    {
        public override string CommandAccessError => "Эта команда доступна админу гильдии (сервера) и владельцу бота!";

        public AdminCommandModule() : base(Categories.Management) { }

        [Command("ban")]
        [Description("Запрещает пользователю скачивать с этого сервера (гильдии)")]
        public async Task Ban(CommandContext ctx,
            [Description("Участник сервера (гильдии)")] DiscordMember discordMember,
            [Description("Причина (Необязательно)"), RemainingText] string reason = null)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{ctx.Member.DisplayName}");

            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = dbCtx.Users.Find(discordMember.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Невозможно забанить незарегистрированного участника!");
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            int isBanned = dbCtx.Bans.Where(x => x.ServerId == discordMember.Guild.Id && x.UserId == discordMember.Id).Count();
            if (isBanned > 0)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Участник уже забанен. (≧◡≦)");
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            DbBan dbBan = new DbBan
            {
                User = dbUser,
                ServerId = discordMember.Guild.Id,
                Reason = reason
            };

            dbCtx.Bans.Add(dbBan);
            await dbCtx.SaveChangesAsync();

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Участник успешно забанен!");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("unban")]
        [Description("Удаляет пользователя из забаненых (пользователю снова разрешено скачивать с этого сервера (гильдии))")]
        public async Task UnBan(CommandContext ctx,
            [Description("Участник сервера (гильдии)")] DiscordMember discordMember)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{ctx.Member.DisplayName}");

            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = dbCtx.Users.Find(discordMember.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Невозможно разбанить незарегистрированного участника!");
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            List<DbBan> dbBanList = dbCtx.Bans.Where(x => x.ServerId == discordMember.Guild.Id && x.UserId == discordMember.Id).ToList();
            if (dbBanList.Count > 0)
            {
                foreach (DbBan dbBan in dbBanList)
                {
                    dbCtx.Bans.Remove(dbBan);
                }
                await dbCtx.SaveChangesAsync();
            }

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Участник успешно разбанен. (≧◡≦)");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("member-ban-reason")]
        [Aliases("m-reason")]
        [Description("Причина бана участника сервера")]
        public async Task MemberBanReason(CommandContext ctx,
            [Description("Участник сервера (гильдии)")] DiscordMember discordMember)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                 .WithTitle($"{ctx.Member.DisplayName}");

            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = dbCtx.Users.Find(discordMember.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription($"Участник {discordMember.DisplayName} не зарегистрирован!");
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            List<DbBan> dbBanList = dbCtx.Bans.Where(x => x.ServerId == discordMember.Guild.Id && x.UserId == discordMember.Id).ToList();
            if (dbBanList.Count > 0)
            {
                discordEmbed.WithColor(DiscordColor.Orange);

                DbBan ban = dbBanList[0];
                if (string.IsNullOrEmpty(ban.Reason))
                {
                    discordEmbed.WithDescription("К сожалению причина бана не была указана.");
                }
                else
                {
                    discordEmbed.WithDescription(ban.Reason);
                }
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription($"Участник {discordMember.DisplayName} не забанен. (≧◡≦)");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("set-art-channel")]
        [Description("Устанавливает канал для поиска сообщений при использовании комманд категории \"Управление коллекциями\"")]
        public async Task SetArtChannel(CommandContext ctx,
            [Description("Канал для поиска сообщений")] DiscordChannel discordChannel)
        {
            YukoDbContext dbCtx = new YukoDbContext();
            DbGuildSettings guildArtChannel = dbCtx.GuildsSettings.Find(ctx.Guild.Id);
            if (guildArtChannel != null)
            {
                guildArtChannel.ArtChannelId = discordChannel.Id;
            }
            else
            {
                dbCtx.GuildsSettings.Add(new DbGuildSettings
                {
                    Id = ctx.Guild.Id,
                    ArtChannelId = discordChannel.Id
                });
            }
            await dbCtx.SaveChangesAsync();
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
               .WithTitle($"{ctx.Member.DisplayName}")
               .WithColor(DiscordColor.Orange)
               .WithDescription("Канал успешно установлен! (≧◡≦)");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("add-command-response")]
        [Aliases("add-response")]
        [Description("Отключает сообщение об успешности выполнения команды add на сервере, взамен сообщение будет приходить в ЛС")]
        public async Task AddCommandResponse(CommandContext ctx,
            [Description("true - включить / false - отключить")] bool isEnabled)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(ctx.Member.DisplayName);

            YukoDbContext dbCtx = new YukoDbContext();
            DbGuildSettings dbGuildSettings = dbCtx.GuildsSettings.Find(ctx.Guild.Id);
            if (dbGuildSettings != null)
            {
                dbGuildSettings.AddCommandResponse = isEnabled;
            }
            else
            {
                dbCtx.GuildsSettings.Add(new DbGuildSettings
                {
                    Id = ctx.Guild.Id,
                    AddCommandResponse = isEnabled
                });
            }
            await dbCtx.SaveChangesAsync();
            discordEmbed.WithColor(DiscordColor.Orange)
                .WithDescription($"{(isEnabled ? "Включено" : "Отключено")}! (≧◡≦)");

            await ctx.RespondAsync(discordEmbed);
        }
    }
}