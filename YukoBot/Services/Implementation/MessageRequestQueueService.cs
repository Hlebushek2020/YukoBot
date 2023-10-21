using DSharpPlus;
using Microsoft.Extensions.Logging;

namespace YukoBot.Services.Implementation;

public class MessageRequestQueueService : IMessageRequestQueueService
{
    #region Fields
    private readonly DiscordClient _discordClient;
    private readonly ILogger<MessageRequestQueueService> _logger;
    #endregion

    public MessageRequestQueueService(DiscordClient discordClient, ILogger<MessageRequestQueueService> logger)
    {
        _discordClient = discordClient;
        _logger = logger;
        
        _logger.LogInformation($"{nameof(MessageRequestQueueService)} loaded.");
    }
}