using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using YukoBot.Models.Database;
using YukoBot.Models.Log;
using YukoBot.Models.Log.Providers;
using YukoBot.Settings;

namespace YukoBot.Modules
{
    internal class BotPingModule : IHandlerModule<MessageCreateEventArgs>
    {
        private readonly ILogger _defaultLogger = YukoLoggerFactory.Current.CreateLogger<DefaultLoggerProvider>();
        private readonly EventId _eventId = new EventId(0, "Ping");

        public async Task Handler(DiscordClient sender, MessageCreateEventArgs e)
        {
            string messageContent = e.Message.Content.Trim();

            if ($"<@{sender.CurrentUser.Id}>".Equals(messageContent) ||
                messageContent.Equals(YukoSettings.Current.BotPrefix, StringComparison.InvariantCultureIgnoreCase))
            {
                _defaultLogger.LogInformation(_eventId, $"{e.Guild.Name}, {e.Channel.Name}, {e.Message.Id}");

                DiscordApplication discordApplication = sender.CurrentApplication;
                bool isOwner = discordApplication.Owners.Any((DiscordUser x) => x.Id == e.Message.Author.Id);
                DiscordMember messageAuthorMember = await e.Message.Channel.Guild.GetMemberAsync(e.Message.Author.Id);

                if (isOwner || new YukoDbContext().Users.Find(e.Message.Author.Id) != null)
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
                    }

                    await e.Message.RespondAsync(
                        $"**Спряталась за {botOwner} и смотрит на {messageAuthorMember.DisplayName}**");
                }
            }
        }
    }
}