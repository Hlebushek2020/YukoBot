using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YukoBot.Extensions;
using YukoBot.Models.Web.Requests;
using YukoBot.Models.Web.Responses;

namespace YukoBot
{
    public partial class YukoBot : IDisposable
    {
        private async Task GetAttachment(ExecuteScriptRequest request, BinaryWriter writer)
        {
            DiscordChannel discordChannel = await _discordClient.GetChannelAsync(request.ChannelId);
            DiscordMessage discordMessage = await discordChannel.GetMessageAsync(request.MessageId);
            UrlsResponse response = new UrlsResponse();
            response.Urls.AddRange(discordMessage.GetImages());
            writer.Write(response.ToString());
        }

        private async Task GetAttachmentsAfter(ExecuteScriptRequest request, BinaryWriter writer)
        {
            DiscordChannel discordChannel = await _discordClient.GetChannelAsync(request.ChannelId);

            int limit = _messageLimit;

            while (request.Count != 0)
            {
                if (request.Count >= limit)
                {
                    request.Count -= limit;
                }
                else
                {
                    limit = request.Count;
                    request.Count = 0;
                }

                IReadOnlyList<DiscordMessage> messages = await discordChannel.GetMessagesAfterAsync(request.MessageId, limit);

                if (messages.Count < _messageLimit)
                {
                    request.Count = 0;
                }

                UrlsResponse response = new UrlsResponse { Next = request.Count > 0 };
                foreach (DiscordMessage message in messages)
                {
                    response.Urls.AddRange(message.GetImages());
                }
                writer.Write(response.ToString());

                if (messages.Count > 0)
                {
                    request.MessageId = messages.First().Id;
                    Thread.Sleep(_messageLimitSleepMs);
                }
            }
        }

        private async Task GetAttacmentsBefore(ExecuteScriptRequest request, BinaryWriter writer)
        {
            DiscordChannel discordChannel = await _discordClient.GetChannelAsync(request.ChannelId);

            int limit = _messageLimit;

            while (request.Count != 0)
            {
                if (request.Count >= limit)
                {
                    request.Count -= limit;
                }
                else
                {
                    limit = request.Count;
                    request.Count = 0;
                }

                IReadOnlyList<DiscordMessage> messages = await discordChannel.GetMessagesBeforeAsync(request.MessageId, limit);

                if (messages.Count < _messageLimit)
                {
                    request.Count = 0;
                }

                UrlsResponse response = new UrlsResponse { Next = request.Count > 0 };
                foreach (DiscordMessage message in messages)
                {
                    response.Urls.AddRange(message.GetImages());
                }
                writer.Write(response.ToString());

                if (messages.Count > 0)
                {
                    request.MessageId = messages.First().Id;
                    Thread.Sleep(_messageLimitSleepMs);
                }
            }
        }

        private async Task GetAttachments(ExecuteScriptRequest request, BinaryWriter writer)
        {
            DiscordChannel discordChannel = await _discordClient.GetChannelAsync(request.ChannelId);

            int limit = _messageLimit;

            if (request.Count >= limit)
            {
                request.Count -= limit;
            }
            else
            {
                limit = request.Count;
                request.Count = 0;
            }

            IReadOnlyList<DiscordMessage> messages = await discordChannel.GetMessagesAsync(limit);

            UrlsResponse response = new UrlsResponse { Next = request.Count > 0 };
            foreach (DiscordMessage message in messages)
            {
                response.Urls.AddRange(message.GetImages());
            }
            writer.Write(response.ToString());

            ulong endId = messages.Last().Id;

            while (request.Count != 0)
            {
                if (request.Count >= limit)
                {
                    request.Count -= limit;
                }
                else
                {
                    limit = request.Count;
                    request.Count = 0;
                }

                Thread.Sleep(_messageLimitSleepMs);

                messages = await discordChannel.GetMessagesBeforeAsync(endId, limit);

                if (messages.Count < _messageLimit)
                {
                    request.Count = 0;
                }
                else
                {
                    endId = messages.Last().Id;
                }

                response = new UrlsResponse { Next = request.Count > 0 };
                foreach (DiscordMessage message in messages)
                {
                    response.Urls.AddRange(message.GetImages());
                }
                writer.Write(response.ToString());
            }
        }
    }
}