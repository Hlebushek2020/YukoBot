using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

namespace YukoBot.Commands
{
    [RequireOwner]
    public class OwnerCommandModule : CommandModule
    {
        public OwnerCommandModule() : base(Models.Category.Management) { }

        [Command("shutdown")]
        [Aliases("sd")]
        [Description("Выключить бота")]
        public async Task Shutdown(CommandContext commandContext)
        {
            await commandContext.RespondAsync("Ok");
            YukoBot.Current.Shutdown();
        }

        [Command("active-time")]
        [Description("Время работы бота")]
        public async Task ActiveTime(CommandContext commandContext)
        {
            TimeSpan timeSpan = DateTime.Now - YukoBot.Current.StartDateTime;
            await commandContext.RespondAsync($"{timeSpan.Days}d, {timeSpan.Hours}h, {timeSpan.Minutes}m, {timeSpan.Seconds}s");
        }

        [Command("set-app")]
        [Description("Устанавливает новую ссылку для команды: app")]
        public async Task SetApp(CommandContext commandContext, string newlink)
        {
            YukoSettings.Current.ClientActualApp = newlink;
            await commandContext.RespondAsync("Ok");
        }
    }
}