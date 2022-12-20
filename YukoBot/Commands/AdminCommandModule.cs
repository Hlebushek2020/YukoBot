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
        [Description("Запретить пользователю скачивать с этого сервера")]
        public async Task Ban(CommandContext ctx,
            [Description("Участник сервера")] DiscordMember member,
            [Description("Причина (необязательно)"), RemainingText] string reason = null)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(ctx.Member.DisplayName);

            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = dbCtx.Users.Find(member.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(Constants.ErrorColor)
                    .WithDescription($"Невозможно забанить незарегистрированного участника! {Constants.SadSmile}");
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            int isBanned = dbCtx.Bans.Where(x => x.ServerId == member.Guild.Id && x.UserId == member.Id).Count();
            if (isBanned > 0)
            {
                discordEmbed
                    .WithColor(Constants.SuccessColor)
                    .WithDescription($"Участник уже забанен! {Constants.HappySmile}");
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            DbBan dbBan = new DbBan
            {
                User = dbUser,
                ServerId = member.Guild.Id,
                Reason = reason
            };

            dbCtx.Bans.Add(dbBan);
            await dbCtx.SaveChangesAsync();

            discordEmbed
                .WithColor(Constants.SuccessColor)
                .WithDescription($"Участник успешно забанен! {Constants.HappySmile}");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("unban")]
        [Description("Удалить пользователя из забаненых (пользователю снова разрешено скачивать с этого сервера)")]
        public async Task UnBan(CommandContext ctx,
            [Description("Участник сервера")] DiscordMember member)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(ctx.Member.DisplayName);

            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = dbCtx.Users.Find(member.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(Constants.ErrorColor)
                    .WithDescription($"Невозможно разбанить незарегистрированного участника! {Constants.SadSmile}");
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            List<DbBan> dbBanList = dbCtx.Bans.Where(x => x.ServerId == member.Guild.Id && x.UserId == member.Id).ToList();
            if (dbBanList.Count > 0)
            {
                foreach (DbBan dbBan in dbBanList)
                {
                    dbCtx.Bans.Remove(dbBan);
                }
                await dbCtx.SaveChangesAsync();
            }

            discordEmbed
                .WithColor(Constants.SuccessColor)
                .WithDescription($"Участник успешно разбанен! {Constants.HappySmile}");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("member-ban-reason")]
        [Aliases("m-reason")]
        [Description("Причина бана участника сервера")]
        public async Task MemberBanReason(CommandContext ctx,
            [Description("Участник сервера")] DiscordMember member)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                 .WithTitle(ctx.Member.DisplayName);

            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = dbCtx.Users.Find(member.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(Constants.ErrorColor)
                    .WithDescription($"Участник {member.DisplayName} не зарегистрирован! {Constants.SadSmile}");
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            discordEmbed.WithColor(Constants.SuccessColor);

            List<DbBan> dbBanList = dbCtx.Bans.Where(x => x.ServerId == member.Guild.Id && x.UserId == member.Id).ToList();
            if (dbBanList.Count > 0)
            {
                DbBan ban = dbBanList[0];
                if (string.IsNullOrEmpty(ban.Reason))
                {
                    discordEmbed.WithDescription($"К сожалению причина бана не была указана. {Constants.SadSmile}");
                }
                else
                {
                    discordEmbed.WithDescription(ban.Reason);
                }
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            discordEmbed.WithDescription($"Участник {member.DisplayName} не забанен! {Constants.HappySmile}");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("set-art-channel")]
        [Description("Установить канал для поиска сообщений для команды `add-by-id`")]
        public async Task SetArtChannel(CommandContext ctx,
            [Description("Канал для поиска сообщений")] DiscordChannel сhannel)
        {
            YukoDbContext dbCtx = new YukoDbContext();
            DbGuildSettings guildArtChannel = dbCtx.GuildsSettings.Find(ctx.Guild.Id);
            if (guildArtChannel != null)
            {
                guildArtChannel.ArtChannelId = сhannel.Id;
            }
            else
            {
                dbCtx.GuildsSettings.Add(new DbGuildSettings
                {
                    Id = ctx.Guild.Id,
                    ArtChannelId = сhannel.Id
                });
            }
            await dbCtx.SaveChangesAsync();
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
               .WithTitle(ctx.Member.DisplayName)
               .WithColor(Constants.SuccessColor)
               .WithDescription($"Канал успешно установлен! {Constants.HappySmile}");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("add-command-response")]
        [Aliases("add-response")]
        [Description("Отправка сообщения об успешности выполнения команды `add` на сервере (взамен сообщение будет приходить в ЛС)")]
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

            discordEmbed.WithColor(Constants.SuccessColor)
                .WithDescription($"{(isEnabled ? "Включено" : "Отключено")}! {Constants.HappySmile}");

            await ctx.RespondAsync(discordEmbed);
        }
    }
}