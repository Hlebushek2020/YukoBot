using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using YukoBot.Commands.Models;
using YukoBot.Extensions;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Commands;

[RequireUserPermissions(Permissions.Administrator)]
public class AdminCommandModule : CommandModule
{
    private readonly YukoDbContext _dbContext;

    public AdminCommandModule(YukoDbContext dbContext) :
        base(Categories.Management, "Простите, эта команда доступна только администратору гильдии!")
    {
        _dbContext = dbContext;
    }

    [Command("notification-channel")]
    [Aliases("notif-channel")]
    [Description("Задать канал для отправки системных уведомлений.")]
    public async Task SetNotificationChannel(
        CommandContext ctx,
        [Description("Канал")]
        DiscordChannel target)
    {
        DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
            .WithHappyMessage(ctx.Member.DisplayName, "Канал успешно установлен!");

        DbGuildSettings guildSettings = await _dbContext.GuildsSettings.FindAsync(ctx.Guild.Id);
        if (guildSettings == null)
        {
            guildSettings = new DbGuildSettings { Id = ctx.Guild.Id };
            _dbContext.GuildsSettings.Add(guildSettings);
        }

        guildSettings.NotificationChannelId = target.Id;

        await _dbContext.SaveChangesAsync();

        await ctx.RespondAsync(discordEmbed);
    }

    [Command("on-notification")]
    [Aliases("on-notif")]
    [Description("Включить / Отключить уведомление о включении бота.")]
    public async Task ReadyNotification(
        CommandContext ctx,
        [Description("true - включить / false - выключить")]
        bool isEnabled)
    {
        DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
            .WithSadMessage(ctx.Member.DisplayName,
                "Простите, я не могу отправлять уведомления о том, когда проснусь. Канал для отправки системных уведомлений не задан!");

        DbGuildSettings guildSettings = await _dbContext.GuildsSettings.FindAsync(ctx.Guild.Id);

        if (guildSettings?.NotificationChannelId != null)
        {
            if (guildSettings.IsReadyNotification != isEnabled)
            {
                guildSettings.IsReadyNotification = isEnabled;
                await _dbContext.SaveChangesAsync();
            }

            discordEmbed.WithHappyMessage(ctx.Member.DisplayName,
                $"Уведомления {(isEnabled ? "включены" : "отключены")}!");
        }

        await ctx.RespondAsync(discordEmbed);
    }

    [Command("off-notification")]
    [Aliases("off-notif")]
    [Description("Включить / Отключить уведомление о выключении бота.")]
    public async Task ShutdownNotification(
        CommandContext ctx,
        [Description("true - включить / false - выключить")]
        bool isEnabled)
    {
        DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
            .WithSadMessage(ctx.Member.DisplayName,
                "Простите, я не могу отправлять уведомления о том, когда пойду баиньки. Канал для отправки системных уведомлений не задан!");

        DbGuildSettings guildSettings = await _dbContext.GuildsSettings.FindAsync(ctx.Guild.Id);

        if (guildSettings?.NotificationChannelId != null)
        {
            if (guildSettings.IsShutdownNotification != isEnabled)
            {
                guildSettings.IsShutdownNotification = isEnabled;
                await _dbContext.SaveChangesAsync();
            }

            discordEmbed.WithHappyMessage(ctx.Member.DisplayName,
                $"Уведомления {(isEnabled ? "включены" : "отключены")}!");
        }

        await ctx.RespondAsync(discordEmbed);
    }
}