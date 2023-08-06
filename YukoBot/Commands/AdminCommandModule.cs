using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using YukoBot.Commands.Attributes;
using YukoBot.Commands.Models;
using YukoBot.Extensions;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Commands
{
    [RequireOwnerAndUserPermissions(Permissions.Administrator)]
    public class AdminCommandModule : CommandModule
    {
        public AdminCommandModule() : base(
            Categories.Management,
            "Простите, эта команда доступна админу гильдии и владельцу бота!") { }

        [Command("ban")]
        [Description("Запретить пользователю скачивать с этого сервера.")]
        public async Task Ban(
            CommandContext ctx,
            [Description("Участник сервера")]
            DiscordMember member,
            [Description("Причина"), RemainingText]
            string reason = "")
        {
            DiscordEmbedBuilder discordEmbed = null;

            if (ctx.User.Id.Equals(member.Id))
            {
                discordEmbed = new DiscordEmbedBuilder()
                    .WithSadMessage(
                        ctx.Member.DisplayName,
                        "Простите, самобан запрещен!");
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = await dbCtx.Users.FindAsync(member.Id);
            if (dbUser == null)
            {
                discordEmbed = new DiscordEmbedBuilder()
                    .WithSadMessage(
                        ctx.Member.DisplayName,
                        "Простите, я не могу забанить незарегистрированного участника!");
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            int isBanned = dbCtx.Bans.Count(x => x.ServerId == member.Guild.Id && x.UserId == member.Id);
            if (isBanned > 0)
            {
                discordEmbed = new DiscordEmbedBuilder()
                    .WithHappyMessage(ctx.Member.DisplayName, "Участник уже забанен!");
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            DbBan dbBan = new DbBan
            {
                User = dbUser,
                ServerId = member.Guild.Id,
                Reason = string.IsNullOrWhiteSpace(reason) ? null : reason
            };

            dbCtx.Bans.Add(dbBan);
            await dbCtx.SaveChangesAsync();

            discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(ctx.Member.DisplayName, "Участник успешно забанен!");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("unban")]
        [Description("Удалить пользователя из забаненых (пользователю снова разрешено скачивать с этого сервера).")]
        public async Task UnBan(
            CommandContext ctx,
            [Description("Участник сервера")]
            DiscordMember member)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder();

            if (ctx.User.Id.Equals(member.Id))
            {
                discordEmbed = new DiscordEmbedBuilder()
                    .WithSadMessage(
                        ctx.Member.DisplayName,
                        "Простите, саморазбан запрещен!");
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = await dbCtx.Users.FindAsync(member.Id);
            if (dbUser == null)
            {
                discordEmbed.WithSadMessage(
                    ctx.Member.DisplayName,
                    "Простите, я не могу разбанить незарегистрированного участника!");
            }
            else
            {
                discordEmbed.WithHappyMessage(ctx.Member.DisplayName, "Участник не забанен!");

                IReadOnlyList<DbBan> dbBanList = dbCtx.Bans
                    .Where(x => x.ServerId == member.Guild.Id && x.UserId == member.Id).ToList();
                if (dbBanList.Count > 0)
                {
                    foreach (DbBan dbBan in dbBanList)
                    {
                        dbCtx.Bans.Remove(dbBan);
                    }
                    await dbCtx.SaveChangesAsync();

                    discordEmbed.WithDescription("Участник успешно разбанен!");
                }
            }

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("member-ban-reason")]
        [Aliases("m-reason")]
        [Description("Причина бана участника сервера.")]
        public async Task MemberBanReason(
            CommandContext ctx,
            [Description("Участник сервера")]
            DiscordMember member)
        {
            DiscordEmbedBuilder discordEmbed = null;

            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = await dbCtx.Users.FindAsync(member.Id);
            if (dbUser == null)
            {
                discordEmbed = new DiscordEmbedBuilder()
                    .WithSadMessage(
                        ctx.Member.DisplayName,
                        $"Простите, участник {member.DisplayName} не зарегистрирован!");
                await ctx.RespondAsync(discordEmbed);
                return;
            }

            discordEmbed = new DiscordEmbedBuilder()
                .WithHappyTitle(ctx.Member.DisplayName)
                .WithColor(Constants.SuccessColor);

            List<DbBan> dbBanList =
                dbCtx.Bans.Where(x => x.ServerId == member.Guild.Id && x.UserId == member.Id).ToList();
            if (dbBanList.Count > 0)
            {
                DbBan ban = dbBanList[0];
                discordEmbed.WithDescription(
                    string.IsNullOrEmpty(ban.Reason)
                        ? "К сожалению причина бана не была указана."
                        : ban.Reason);
            }
            else
            {
                discordEmbed.WithDescription($"Участник {member.DisplayName} не забанен!");
            }

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("set-art-channel")]
        [Description("Установить канал для поиска сообщений для команды `add-by-id`.")]
        public async Task SetArtChannel(
            CommandContext ctx,
            [Description("Канал для поиска сообщений")]
            DiscordChannel сhannel)
        {
            YukoDbContext dbCtx = new YukoDbContext();
            DbGuildSettings guildArtChannel = await dbCtx.GuildsSettings.FindAsync(ctx.Guild.Id);
            if (guildArtChannel != null)
            {
                guildArtChannel.ArtChannelId = сhannel.Id;
            }
            else
            {
                dbCtx.GuildsSettings.Add(
                    new DbGuildSettings
                    {
                        Id = ctx.Guild.Id,
                        ArtChannelId = сhannel.Id
                    });
            }
            await dbCtx.SaveChangesAsync();

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(ctx.Member.DisplayName, "Канал для поиска сообщений успешно установлен!");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("add-command-response")]
        [Aliases("add-response")]
        [Description(
            "Отправка сообщения об успешности выполнения команды `add` на сервере (сообщение будет приходить в ЛС, а не в канал где выполнена команда).")]
        public async Task AddCommandResponse(
            CommandContext ctx,
            [Description("true - включить / false - отключить")]
            bool isEnabled)
        {
            YukoDbContext dbCtx = new YukoDbContext();
            DbGuildSettings dbGuildSettings = await dbCtx.GuildsSettings.FindAsync(ctx.Guild.Id);
            if (dbGuildSettings != null)
            {
                dbGuildSettings.AddCommandResponse = isEnabled;
            }
            else
            {
                dbCtx.GuildsSettings.Add(
                    new DbGuildSettings
                    {
                        Id = ctx.Guild.Id,
                        AddCommandResponse = isEnabled
                    });
            }
            await dbCtx.SaveChangesAsync();

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(ctx.Member.DisplayName, isEnabled ? "Включено!" : "Отключено!");
            await ctx.RespondAsync(discordEmbed);
        }
    }
}