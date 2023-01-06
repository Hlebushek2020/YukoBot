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
using YukoBot.Settings;

namespace YukoBot.Commands
{
    [RequireOwner]
    public class OwnerCommandModule : CommandModule
    {
        public override string CommandAccessError => "Простите, эта команда доступна только владельцу бота!";

        public OwnerCommandModule() : base(Categories.Management)
        {
        }

        [Command("shutdown")]
        [Aliases("sd")]
        [Description("Выключить бота.")]
        public async Task Shutdown(CommandContext ctx)
        {
            await ctx.RespondAsync($"Хорошо, хозяин! {Constants.HappySmile}");
            YukoBot.Current.Shutdown();
        }

        [Command("status")]
        [Description("Сведения о боте.")]
        public async Task Status(CommandContext ctx)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(Constants.StatusColor)
                .AddField("Net", $"v{Environment.Version}")
                .AddField("Сборка",
                    $"v{Assembly.GetExecutingAssembly().GetName().Version} {File.GetCreationTime(Assembly.GetExecutingAssembly().Location):dd.MM.yyyy}")
                .AddField("Дата запуска",
                    $"{YukoBot.Current.StartDateTime:dd.MM.yyyy} {YukoBot.Current.StartDateTime:HH:mm:ss zzz}");

            TimeSpan timeSpan = DateTime.Now - YukoBot.Current.StartDateTime;
            discordEmbed.AddField("Время работы",
                $"{timeSpan.Days}d, {timeSpan.Hours}h, {timeSpan.Minutes}m, {timeSpan.Seconds}s");

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("set-app")]
        [Description("Установить новую ссылку для команды `app`.")]
        public async Task SetApp(CommandContext ctx, string newlink)
        {
            YukoSettings.Current.SetApp(newlink);
            await ctx.RespondAsync($"Хозяин! Ссылка на приложение установлена! {Constants.HappySmile}");
        }

        [Command("set-premium")]
        [Description("Предоставление пользователю дополнительных возможностей.")]
        public async Task SetPremium(CommandContext ctx,
            [Description("Участник сервера (гильдии)")]
            DiscordMember discordMember,
            [Description("true - предоставить / false - отобрать")]
            bool isEnabled)
        {
            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = dbCtx.Users.Find(discordMember.Id);
            if (dbUser != null)
            {
                dbUser.HasPremium = isEnabled;
                await dbCtx.SaveChangesAsync();
                await ctx.RespondAsync($"{(isEnabled ? "Предоставлено" : "Отобрано")}! {Constants.HappySmile}");
            }
            else
            {
                await ctx.RespondAsync($"Хозяин! Данный участник сервера не зарегистрирован! {Constants.SadSmile}");
            }
        }
    }
}