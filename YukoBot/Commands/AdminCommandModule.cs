using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YukoBot.Commands.Attribute;
using YukoBot.Models.Database;

namespace YukoBot.Commands
{
    [RequireOwnerAndUserPermissions(Permissions.Administrator)]
    public class AdminCommandModule : CommandModule
    {
        public AdminCommandModule()
        {
            ModuleName = "Команды управления";
        }

        [Command("ban")]
        [Description("Запрещает пользователю скачивать с этого сервера (гильдии)")]
        public async Task Ban(CommandContext commandContext,
            [Description("Участник сервера (гильдии)")] DiscordMember discordMember,
            [Description("Причина (Необязательно)"), RemainingText] string reason = null)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{commandContext.Member.DisplayName}");

            YukoDbContext database = new YukoDbContext();
            DbUser dbUser = database.Users.Find(discordMember.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Невозможно забанить незарегистрированного участника!");
                await commandContext.RespondAsync(discordEmbed);
                return;
            }

            int isBanned = database.Bans.Where(x => x.ServerId == discordMember.Guild.Id && x.UserId == discordMember.Id).Count();
            if (isBanned > 0)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Участник уже забанен. ≧◡≦");
                await commandContext.RespondAsync(discordEmbed);
                return;
            }

            DbBan dbBan = new DbBan
            {
                User = dbUser,
                ServerId = discordMember.Guild.Id,
                Reason = reason
            };

            database.Bans.Add(dbBan);
            await database.SaveChangesAsync();

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Участник успешно забанен!");
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("unban")]
        [Description("Удаляет пользователя из забаненых (пользователю снова разрешено скачивать с этого сервера (гильдии))")]
        public async Task UnBan(CommandContext commandContext, [Description("Участник сервера (гильдии)")] DiscordMember discordMember)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{commandContext.Member.DisplayName}");

            YukoDbContext database = new YukoDbContext();
            DbUser dbUser = database.Users.Find(discordMember.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Невозможно разбанить незарегистрированного участника!");
                await commandContext.RespondAsync(discordEmbed);
                return;
            }

            List<DbBan> currentBans = database.Bans.Where(x => x.ServerId == discordMember.Guild.Id && x.UserId == discordMember.Id).ToList();
            if (currentBans.Count > 0)
            {
                foreach (DbBan dbBan in currentBans)
                {
                    database.Bans.Remove(dbBan);
                }
                await database.SaveChangesAsync();
            }

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Участник успешно разбанен. ≧◡≦");
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("member-ban-reason")]
        [Aliases("m-reason")]
        [Description("Причина бана участника сервера")]
        public async Task MemberBanReason(CommandContext commandContext, [Description("Участник сервера (гильдии)")] DiscordMember discordMember)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                 .WithTitle($"{commandContext.Member.DisplayName}");

            YukoDbContext database = new YukoDbContext();
            DbUser dbUser = database.Users.Find(discordMember.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription($"Участник {discordMember.DisplayName} не зарегистрирован!");
                await commandContext.RespondAsync(discordEmbed);
                return;
            }

            List<DbBan> currentBans = database.Bans.Where(x => x.ServerId == discordMember.Guild.Id && x.UserId == discordMember.Id).ToList();
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
                .WithDescription($"Участник {discordMember.DisplayName} не забанен. ≧◡≦");
            await commandContext.RespondAsync(discordEmbed);
        }
    }
}