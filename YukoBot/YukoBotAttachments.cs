using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YukoBot.Extensions;
using YukoBot.Models.Database.Entities;
using YukoBot.Models.Web.Requests;
using YukoBot.Models.Web.Responses;

namespace YukoBot
{
    public partial class YukoBot : IDisposable
    {
        private async Task GetAttachment(ExecuteScriptRequest request, BinaryWriter writer)
        {
            DiscordChannel discordChannel = await discordClient.GetChannelAsync(request.ChannelId);
            DiscordMessage discordMessage = await discordChannel.GetMessageAsync(request.MessageId);
            UrlsResponse response = new UrlsResponse();
            //response.Urls.AddRange(discordMessage.Attachments.Select(x => x.Url));
            //response.Urls.AddRange(discordMessage.Embeds.Where(x => x.Image != null).Select(x => x.Image.Url.ToString()));
            response.Urls.AddRange(discordMessage.GetImages());
            writer.Write(response.ToString());
        }

        private async Task GetAttachmentsAfter(DbUser dbUser, ExecuteScriptRequest request, BinaryWriter writer)
        {
            DiscordChannel discordChannel = await discordClient.GetChannelAsync(request.ChannelId);

            int limit = messageLimit;

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

                if (messages.Count < messageLimit)
                {
                    request.Count = 0;
                }

                UrlsResponse response = new UrlsResponse { Next = request.Count > 0 };
                foreach (DiscordMessage message in messages)
                {
                    //response.Urls.AddRange(message.Attachments.Select(x => x.Url));
                    //response.Urls.AddRange(message.Embeds.Where(x => x.Image != null).Select(x => x.Image.Url.ToString()));
                    response.Urls.AddRange(message.GetImages());
                }
                writer.Write(response.ToString());

                if (messages.Count > 0)
                {
                    request.MessageId = messages.First().Id;
                    Thread.Sleep(messageLimitSleepMs);
                }
            }
        }

        private async Task GetAttacmentsBefore(DbUser dbUser, ExecuteScriptRequest request, BinaryWriter writer)
        {
            DiscordChannel discordChannel = await discordClient.GetChannelAsync(request.ChannelId);

            int limit = messageLimit;

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

                if (messages.Count < messageLimit)
                {
                    request.Count = 0;
                }

                UrlsResponse response = new UrlsResponse { Next = request.Count > 0 };
                foreach (DiscordMessage message in messages)
                {
                    //response.Urls.AddRange(message.Attachments.Select(x => x.Url));
                    //response.Urls.AddRange(message.Embeds.Where(x => x.Image != null).Select(x => x.Image.Url.ToString()));
                    response.Urls.AddRange(message.GetImages());
                }
                writer.Write(response.ToString());

                if (messages.Count > 0)
                {
                    request.MessageId = messages.First().Id;
                    Thread.Sleep(messageLimitSleepMs);
                }
            }
        }

        private async Task GetAttachments(DbUser dbUser, ExecuteScriptRequest request, BinaryWriter writer)
        {
            DiscordChannel discordChannel = await discordClient.GetChannelAsync(request.ChannelId);

            int limit = messageLimit;

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
                //response.Urls.AddRange(message.Attachments.Select(x => x.Url));
                //response.Urls.AddRange(message.Embeds.Where(x => x.Image != null).Select(x => x.Image.Url.ToString()));
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

                Thread.Sleep(messageLimitSleepMs);

                messages = await discordChannel.GetMessagesBeforeAsync(endId, limit);

                if (messages.Count < messageLimit)
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
                    //response.Urls.AddRange(message.Attachments.Select(x => x.Url));
                    //response.Urls.AddRange(message.Embeds.Where(x => x.Image != null).Select(x => x.Image.Url.ToString()));
                    response.Urls.AddRange(message.GetImages());
                }
                writer.Write(response.ToString());
            }
        }
    }
}