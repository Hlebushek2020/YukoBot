using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace YukoBot.Services;

public interface IBotPingService
{
    Task Handler(DiscordClient sender, MessageCreateEventArgs e);
}