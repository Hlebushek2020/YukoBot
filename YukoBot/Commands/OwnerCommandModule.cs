using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using YukoBot.Commands.Models;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Commands
{
    [RequireOwner]
    public class OwnerCommandModule : CommandModule
    {
        public override string CommandAccessError => "Эта команда доступна только владельцу бота!";

        public OwnerCommandModule() : base(Categories.Management) { }

        [Command("shutdown")]
        [Aliases("sd")]
        [Description("Выключить бота")]
        public async Task Shutdown(CommandContext ctx)
        {
            await ctx.RespondAsync("Ok");
            YukoBot.Current.Shutdown();
        }

        [Command("status")]
        [Description("Сведения о боте")]
        public async Task Status(CommandContext ctx)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
            {
                Color = DiscordColor.DarkGray
            };

            discordEmbed.AddField("Net", $"v{Environment.Version}");
            discordEmbed.AddField("Сборка", $"v{Assembly.GetExecutingAssembly().GetName().Version} {File.GetCreationTime(Assembly.GetExecutingAssembly().Location):dd.MM.yyyy}");
            discordEmbed.AddField("Дата запуска", $"{YukoBot.Current.StartDateTime:dd.MM.yyyy} {YukoBot.Current.StartDateTime:HH:mm:ss zzz}");
            TimeSpan timeSpan = DateTime.Now - YukoBot.Current.StartDateTime;
            discordEmbed.AddField("Время работы", $"{timeSpan.Days}d, {timeSpan.Hours}h, {timeSpan.Minutes}m, {timeSpan.Seconds}s");

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("set-app")]
        [Description("Устанавливает новую ссылку для команды: app")]
        public async Task SetApp(CommandContext ctx, string newlink)
        {
            YukoSettings.Current.SetApp(newlink);
            await ctx.RespondAsync("Ok");
        }

        [Command("set-premium")]
        [Description("Предоставляет пользователю дополнительные возможности")]
        public async Task SetPremium(CommandContext ctx,
            [Description("Участник сервера (гильдии)")] DiscordMember discordMember,
            [Description("true - включить / false - отключить")] bool isEnabled)
        {
            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = dbCtx.Users.Find(discordMember.Id);
            if (dbUser != null)
            {
                dbUser.HasPremium = isEnabled;
                await dbCtx.SaveChangesAsync();
                await ctx.RespondAsync(isEnabled ? "Включено" : "Отключено");
            }
            else
            {
                await ctx.RespondAsync("Пользователь не найден");
            }
        }
    }
}