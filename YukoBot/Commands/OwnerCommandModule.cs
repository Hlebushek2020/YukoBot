using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using YukoBot.Commands.Models;

namespace YukoBot.Commands
{
    [RequireOwner]
    public class OwnerCommandModule : CommandModule
    {
        public OwnerCommandModule() : base(Categories.Management)
        {
            CommandAccessError = "Эта команда доступна только владельцу бота!";
        }

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

            discordEmbed.AddField("Сборка", $"v{Assembly.GetExecutingAssembly().GetName().Version} {File.GetCreationTime(Assembly.GetExecutingAssembly().Location).ToShortDateString()} .net {Environment.Version}");
            discordEmbed.AddField("Дата запуска", $"{YukoBot.Current.StartDateTime.ToShortDateString()} {YukoBot.Current.StartDateTime.ToShortTimeString()}");
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
    }
}