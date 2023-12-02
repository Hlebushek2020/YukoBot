using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace YukoBot.Services;

public interface IMessageRequestQueueService
{
    Task<IAsyncEnumerable<DiscordMessage>> GetMessagesAfterAsync(DiscordChannel channel, ulong after, int limit);
    Task<IAsyncEnumerable<DiscordMessage>> GetMessagesBeforeAsync(DiscordChannel channel, ulong before, int limit);
    Task<IAsyncEnumerable<DiscordMessage>> GetMessagesAsync(DiscordChannel channel, int limit);
    Task<DiscordMessage> GetMessageAsync(DiscordChannel channel, ulong messageId);
    Task StopProcessing();
}