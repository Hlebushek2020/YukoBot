using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace YukoBot.Services;

public interface IMessageRequestQueueService
{
    Task<IReadOnlyList<DiscordMessage>> GetMessagesAfterAsync(DiscordChannel channel, ulong after, int limit);
    Task<IReadOnlyList<DiscordMessage>> GetMessagesBeforeAsync(DiscordChannel channel, ulong before, int limit);
    Task<IReadOnlyList<DiscordMessage>> GetMessagesAsync(DiscordChannel channel, int limit);
    Task StopProcessing();
}