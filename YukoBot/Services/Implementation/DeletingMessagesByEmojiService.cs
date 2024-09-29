using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace YukoBot.Services.Implementation
{
    internal class DeletingMessagesByEmojiService : IDeletingMessagesByEmojiService
    {
        private readonly ILogger<DeletingMessagesByEmojiService> _logger;

        public DeletingMessagesByEmojiService(
            DiscordClient discordClient,
            ILogger<DeletingMessagesByEmojiService> logger)
        {
            _logger = logger;

            discordClient.MessageReactionAdded += Handler;

            _logger.LogInformation($"{nameof(DeletingMessagesByEmojiService)} loaded.");
        }

        public async Task Handler(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            DiscordEmoji emoji = DiscordEmoji.FromName(sender, Constants.DeleteMessageEmoji, false);

            if (e.Emoji.Equals(emoji) && !e.User.Id.Equals(sender.CurrentUser.Id))
            {
                DiscordMessage discordMessage = await e.Channel.GetMessageAsync(e.Message.Id);
                if (discordMessage.Author.Id.Equals(sender.CurrentUser.Id))
                {
                    _logger.LogInformation(
                        e.Guild != null
                            ? $"Guild: {e.Guild.Name} ({e.Guild.Id}). Channel: {e.Channel.Name} ({e.Channel.Id})."
                            : $"Username: {e.User.Username}");

                    if (e.Guild != null)
                    {
                        DiscordMember discordMember = await e.Guild.GetMemberAsync(e.User.Id);
                        if (discordMember.Permissions.HasPermission(Permissions.Administrator) ||
                            e.Message.ReferencedMessage?.Author.Id == e.User.Id && discordMessage.IsTTS)
                        {
                            await e.Message.DeleteAsync();
                        }
                    }
                    else
                    {
                        await e.Message.DeleteAsync();
                    }
                }
            }
        }
    }
}