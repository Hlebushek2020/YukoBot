using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace YukoBot.Services.Implementation;

public class MessageRequestQueueService : IMessageRequestQueueService
{
    #region Fields
    private readonly IYukoSettings _settings;

    private readonly ConcurrentQueue<QueueEntry> _items;
    private readonly CancellationTokenSource _cts;

    private Task _processTask;
    #endregion

    public MessageRequestQueueService(IYukoSettings settings, ILogger<MessageRequestQueueService> logger)
    {
        _settings = settings;

        _items = new ConcurrentQueue<QueueEntry>();
        _cts = new CancellationTokenSource();

        logger.LogInformation($"{nameof(MessageRequestQueueService)} loaded.");
    }

    public Task<IReadOnlyList<DiscordMessage>> GetMessagesAfterAsync(DiscordChannel channel, ulong after, int limit) =>
        GetMessagesInternal(new QueueEntry(channel, limit, null, after));

    public Task<IReadOnlyList<DiscordMessage>>
        GetMessagesBeforeAsync(DiscordChannel channel, ulong before, int limit) =>
        GetMessagesInternal(new QueueEntry(channel, limit, before, null));

    public Task<IReadOnlyList<DiscordMessage>> GetMessagesAsync(DiscordChannel channel, int limit) =>
        GetMessagesInternal(new QueueEntry(channel, limit, null, null));

    private Task<IReadOnlyList<DiscordMessage>> GetMessagesInternal(QueueEntry queueEntry)
    {
        if (_cts.IsCancellationRequested)
            throw new Exception("MessageRequestQueueService STOP");

        _items.Enqueue(queueEntry);
        _processTask ??= Task.Run(Process, _cts.Token);
        return queueEntry.ResultTask();
    }

    public Task StopProcessing()
    {
        _cts.Cancel();
        return _processTask;
    }

    private void Process()
    {
        while (!_cts.IsCancellationRequested || _items?.Count > 0)
        {
            if (!_items.TryDequeue(out QueueEntry container))
                continue;

            container.ExecuteAsync();
            Thread.Sleep(_settings.IntervalBetweenMessageRequests);
        }
    }

    private class QueueEntry
    {
        private readonly DiscordChannel _discordChannel;
        private readonly int _limit;
        private readonly ulong? _before;
        private readonly ulong? _after;

        private readonly TaskCompletionSource<IReadOnlyList<DiscordMessage>> _tcs;

        public QueueEntry(DiscordChannel discordChannel, int limit, ulong? before, ulong? after)
        {
            _discordChannel = discordChannel;
            _limit = limit;
            _before = before;
            _after = after;

            _tcs = new TaskCompletionSource<IReadOnlyList<DiscordMessage>>();
        }

        public Task<IReadOnlyList<DiscordMessage>> ResultTask() => _tcs.Task;

        public async void ExecuteAsync()
        {
            try
            {
                IReadOnlyList<DiscordMessage> messages;
                if (_before.HasValue)
                {
                    messages = await _discordChannel.GetMessagesBeforeAsync(_before.Value, _limit);
                }
                else if (_after.HasValue)
                {
                    messages = await _discordChannel.GetMessagesAfterAsync(_after.Value, _limit);
                }
                else
                {
                    messages = await _discordChannel.GetMessagesAsync(_limit);
                }
                _tcs.SetResult(messages);
            }
            catch (Exception e)
            {
                _tcs.SetException(e);
            }
        }
    }
}