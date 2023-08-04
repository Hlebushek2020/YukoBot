using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace YukoBot.Services;

public interface IDeletingMessagesByEmojiService
{
    Task Handler(DiscordClient sender, MessageReactionAddEventArgs e);
}