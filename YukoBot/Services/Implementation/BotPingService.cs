using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using YukoBot.Models.Database;

namespace YukoBot.Services.Implementation
{
    internal class BotPingService : IBotPingService
    {
        private readonly YukoDbContext _dbContext;
        private readonly IYukoSettings _yukoSettings;
        private readonly ILogger<BotPingService> _logger;

        public BotPingService(
            DiscordClient discordClient,
            YukoDbContext dbContext,
            IYukoSettings yukoSettings,
            ILogger<BotPingService> logger)
        {
            _dbContext = dbContext;
            _yukoSettings = yukoSettings;
            _logger = logger;

            discordClient.MessageCreated += Handler;

            _logger.LogInformation($"{nameof(BotPingService)} loaded.");
        }

        public async Task Handler(DiscordClient sender, MessageCreateEventArgs e)
        {
            string messageContent = e.Message.Content.Trim();

            if ($"<@{sender.CurrentUser.Id}>".Equals(messageContent) ||
                messageContent.Equals(_yukoSettings.BotPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogInformation($"{e.Guild.Name}, {e.Channel.Name}, {e.Message.Id}");

                DiscordApplication discordApplication = sender.CurrentApplication;
                bool isOwner = discordApplication.Owners.Any(x => x.Id == e.Message.Author.Id);
                DiscordMember messageAuthorMember = await e.Message.Channel.Guild.GetMemberAsync(e.Message.Author.Id);

                if (isOwner || await _dbContext.Users.FindAsync(e.Message.Author.Id) != null)
                {
                    await e.Message.RespondAsync($"**Подбежала и обняла {messageAuthorMember.DisplayName}**");
                }
                else
                {
                    DiscordUser botOwnerUser = discordApplication.Owners.First();
                    string botOwner = $"{botOwnerUser.Username}#{botOwnerUser.Discriminator}";

                    try
                    {
                        DiscordMember botOwnerMember = await e.Message.Channel.Guild.GetMemberAsync(botOwnerUser.Id);
                        botOwner = botOwnerMember.DisplayName;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    await e.Message.RespondAsync(
                        $"**Спряталась за {botOwner} и смотрит на {messageAuthorMember.DisplayName}**");
                }
            }
        }
    }
}