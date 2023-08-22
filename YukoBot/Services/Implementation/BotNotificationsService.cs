using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Services.Implementation
{
    internal class BotNotificationsService : IBotNotificationsService
    {
        private readonly DiscordClient _discordClient;
        private readonly YukoDbContext _dbContext;
        private readonly ILogger<BotNotificationsService> _logger;

        public BotNotificationsService(
            DiscordClient discordClient,
            YukoDbContext dbContext,
            ILogger<BotNotificationsService> logger)
        {
            _dbContext = dbContext;
            _discordClient = discordClient;
            _logger = logger;

            _logger.LogInformation($"{nameof(BotNotificationsService)} loaded.");
        }

        public Task SendReadyNotifications()
        {
            _logger.LogInformation("Sending notifications about the start of the bot.");
            return SendNotifications(
                "Новый день - новая возможность! Я готова вас радовать!",
                gs => gs.IsReadyNotification);
        }

        public Task SendShutdownNotifications(string reason)
        {
            _logger.LogInformation("Sending notifications about the shutdown of the bot.");
            return SendNotifications(
                $"Хорошего дня (вечера)! Хозяин сказал мне отдыхать по причине: {reason}",
                gs => gs.IsShutdownNotification);
        }

        private async Task SendNotifications(string message, Expression<Func<DbGuildSettings, bool>> predicate)
        {
            IReadOnlyList<DbGuildSettings> guildSettingsList =
                await _dbContext.GuildsSettings.Where(predicate).ToListAsync();

            DiscordEmbed discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(_discordClient.CurrentUser.Username)
                .WithDescription(message)
                .WithColor(Constants.SuccessColor)
                .Build();

            foreach (DbGuildSettings guildSettings in guildSettingsList)
            {
                try
                {
                    DiscordChannel discordChannel =
                        await _discordClient.GetChannelAsync(guildSettings.NotificationChannelId.Value);
                    await discordChannel.SendMessageAsync(discordEmbed);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        $"Notification not sent. Guild: {guildSettings.Id}. Channel: {
                            guildSettings.NotificationChannelId}. Message: {ex.Message}");
                }
            }
        }
    }
}