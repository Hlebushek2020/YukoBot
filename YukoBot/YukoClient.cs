using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using YukoBot.Enums;
using YukoBot.Extensions;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;
using YukoBot.Models.Database.JoinedEntities;
using YukoBot.Models.Json;
using YukoBot.Models.Json.Errors;
using YukoBot.Models.Json.Requests;
using YukoBot.Models.Json.Responses;
using YukoBot.Services;

namespace YukoBot
{
    public class YukoClient : IDisposable
    {
        #region Fields
        private static volatile int _countClient = 0;
        private static readonly ConcurrentDictionary<Guid, ulong> _userTokens = new ConcurrentDictionary<Guid, ulong>();

        private readonly DiscordClient _discordClient;
        private readonly ILogger<YukoClient> _logger;
        private readonly TcpClient _tcpClient;
        private readonly IYukoSettings _yukoSettings;
        private readonly YukoDbContext _dbContext;
        private readonly IMessageRequestQueueService _messageRequestQueue;
        private readonly string _endPoint;

        private bool _isDisposed = false;
        private BinaryReader _binaryReader = null;
        private BinaryWriter _binaryWriter = null;
        private DbUser _currentDbUser = null;
        #endregion

        public static bool Availability => _countClient > 0;

        public YukoClient(IServiceProvider services, TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _endPoint = _tcpClient.Client.RemoteEndPoint.ToString();

            _discordClient = services.GetService<DiscordClient>();
            _logger = services.GetService<ILogger<YukoClient>>();
            _dbContext = services.GetService<YukoDbContext>();
            _yukoSettings = services.GetService<IYukoSettings>();
            _messageRequestQueue = services.GetService<IMessageRequestQueueService>();
        }

        public async void Process(object args)
        {
            Interlocked.Increment(ref _countClient);
            try
            {
                _logger.LogInformation($"[{_endPoint}] Connected");
                NetworkStream networkStream = _tcpClient.GetStream();
                _binaryReader = new BinaryReader(networkStream, Encoding.UTF8, true);
                _binaryWriter = new BinaryWriter(networkStream, Encoding.UTF8, true);
                RequestType requestType = (RequestType) _binaryReader.ReadInt32();
                _logger.LogDebug($"[{_endPoint}] Request type: {requestType}");
                if (requestType != RequestType.Authorization)
                {
                    Guid userToken = Guid.Parse(_binaryReader.ReadString());
                    _currentDbUser = await _dbContext.Users.FindAsync(
                        _userTokens.TryGetValue(userToken, out ulong userId) ? userId : ulong.MinValue);
                    if (_currentDbUser == null)
                    {
                        _binaryWriter.Write(new Response<BaseErrorJson>
                        {
                            Error = new BaseErrorJson { Code = ClientErrorCodes.NotAuthorized }
                        }.ToString());
                    }
                    else
                    {
                        switch (requestType)
                        {
                            case RequestType.GetServer:
                                await ClientGetServer();
                                break;
                            case RequestType.GetServers:
                                await ClientGetServers();
                                break;
                            case RequestType.ExecuteScripts:
                                await ClientExecuteScripts();
                                break;
                            case RequestType.GetMessageCollections:
                                ClientGetMessageCollections();
                                break;
                            case RequestType.GetUrls:
                                await ClientGetUrls();
                                break;
                        }
                    }
                }
                else
                {
                    await ClientAuthorization();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[{_endPoint}] {ex.Message}");
            }
            finally
            {
                Interlocked.Decrement(ref _countClient);
                Dispose();
                _logger.LogInformation($"[{_endPoint}] Disconnected");
            }
        }

        #region Requests
        private async Task ClientAuthorization()
        {
            AuthorizationRequest request = AuthorizationRequest.FromJson(_binaryReader.ReadString());
            DbUser dbUser = _dbContext.Users.FirstOrDefault(x => x.Username == request.Login);
            if (dbUser is null)
            {
                if (ulong.TryParse(request.Login, out ulong id))
                    dbUser = _dbContext.Users.FirstOrDefault(x => x.Id == id);
            }
            if (dbUser == null || !dbUser.Password.Equals(request.Password))
            {
                _binaryWriter.Write(new AuthorizationResponse
                {
                    Error = new BaseErrorJson { Code = ClientErrorCodes.InvalidCredentials }
                }.ToString());
            }
            else
            {
                // save db
                dbUser.LastLogin = DateTime.Now;
                await _dbContext.SaveChangesAsync();
                // add token to dictionary
                Guid userToken = Guid.NewGuid();
                _userTokens.AddOrUpdate(userToken, x => dbUser.Id, (x, y) => dbUser.Id);
                // response build
                DiscordUser discordUser = await _discordClient.GetUserAsync(dbUser.Id);
                _binaryWriter.Write(new AuthorizationResponse
                {
                    UserId = discordUser.Id,
                    Username = discordUser.Username,
                    AvatarUri = discordUser.AvatarUrl,
                    Token = userToken
                }.ToString());
            }
        }

        private async Task ClientGetServer()
        {
            ServerRequest request = ServerRequest.FromJson(_binaryReader.ReadString());
            DiscordGuild guild = await _discordClient.GetGuildAsync(request.Id);
            ServerResponse response = ServerResponse.FromServerJson(await GetServer(guild));
            _binaryWriter.Write(response.ToString());
        }

        private async Task ClientGetServers()
        {
            ServersResponse response = new ServersResponse();
            foreach (KeyValuePair<ulong, DiscordGuild> guild in _discordClient.Guilds)
            {
                if (guild.Value.Members.Any(x => x.Value.Id == _currentDbUser.Id))
                {
                    ServerJson server = await GetServer(guild.Value);
                    if (server != null)
                        response.Servers.Add(server);
                }
            }
            _binaryWriter.Write(response.ToString());
        }

        private async Task ClientExecuteScripts()
        {
            ServerRequest serverRequest = ServerRequest.FromJson(_binaryReader.ReadString());
            DbBan ban = _dbContext.Bans.FirstOrDefault(
                x =>
                    x.UserId == _currentDbUser.Id &&
                    x.ServerId == serverRequest.Id);
            if (ban != null)
            {
                _binaryWriter.Write(new Response<ExecuteScriptErrorJson>
                {
                    Error = new ExecuteScriptErrorJson
                    {
                        Code = ClientErrorCodes.MemberBanned,
                        Reason = ban.Reason
                    }
                }.ToString());
            }
            else
            {
                DiscordGuild guild = await _discordClient.GetGuildAsync(serverRequest.Id);
                DiscordMember isContainsMember = await guild.GetMemberAsync(_currentDbUser.Id, true);
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (isContainsMember != null)
                {
                    _binaryWriter.Write(new Response<BaseErrorJson>().ToString());
                    ExecuteScriptRequest scriptRequest;
                    do
                    {
                        string jsonString = _binaryReader.ReadString();
                        scriptRequest = ExecuteScriptRequest.FromJson(jsonString);
                        _logger.LogDebug($"[{_endPoint}] Execute script request: {jsonString}");
                        try
                        {
                            switch (scriptRequest.Mode)
                            {
                                case ScriptMode.One:
                                    await GetAttachment(scriptRequest);
                                    break;
                                case ScriptMode.After:
                                    await GetAttachmentsAfter(scriptRequest);
                                    break;
                                case ScriptMode.Before:
                                    await GetAttachmentsBefore(scriptRequest);
                                    break;
                                case ScriptMode.End:
                                    await GetAttachments(scriptRequest);
                                    break;
                                case ScriptMode.All:
                                    scriptRequest.Count = int.MaxValue;
                                    await GetAttachments(scriptRequest);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            _binaryWriter.Write(new UrlsResponse
                            {
                                Next = false,
                                Error = new UrlsErrorJson { Code = ClientErrorCodes.UnhandledException }
                            }.ToString());
                            _logger.LogError(ex, $"[{_endPoint}] [{_currentDbUser.Id}] {ex.Message}");
                        }
                    } while (scriptRequest.HasNext);
                }
                else
                {
                    _binaryWriter.Write(new Response<BaseErrorJson>
                    {
                        Error = new BaseErrorJson
                        {
                            Code = ClientErrorCodes.MemberNotFound
                        }
                    }.ToString());
                }
            }
        }

        private void ClientGetMessageCollections()
        {
            IReadOnlyList<DbCollection> dbCollections =
                _dbContext.Collections.Where(x => x.UserId == _currentDbUser.Id).ToList();
            MessageCollectionsResponse response = new MessageCollectionsResponse();
            foreach (DbCollection dbCollection in dbCollections)
            {
                MessageCollectionJson collection = new MessageCollectionJson
                {
                    Name = dbCollection.Name,
                    Id = dbCollection.Id
                };
                IQueryable<DbMessage> dbCollectionItems = _dbContext.CollectionItems
                    .Where(x => x.CollectionId == dbCollection.Id)
                    .Join(
                        _dbContext.Messages,
                        ci => ci.MessageId,
                        m => m.Id,
                        (ci, m) => new DbMessage
                            { Id = m.Id, ChannelId = m.ChannelId });
                foreach (DbMessage dbMessage in dbCollectionItems)
                {
                    collection.Items.Add(
                        new MessageCollectionItemJson
                        {
                            ChannelId = dbMessage.ChannelId,
                            MessageId = dbMessage.Id
                        });
                }
                response.MessageCollections.Add(collection);
            }
            _binaryWriter.Write(response.ToString());
        }

        private async Task ClientGetUrls(string requestString)
        {
            UrlsResponse response;
            UrlsRequest request = UrlsRequest.FromJson(requestString);
            Dictionary<ulong, CollectionItemJoinMessage> collectionItems = _dbContext.CollectionItems
                .Where(ci => ci.CollectionId == request.Id)
                .Join(
                    _dbContext.Messages,
                    ci => ci.MessageId,
                    m => m.Id,
                    (ci, m) => new CollectionItemJoinMessage
                    {
                        MessageId = m.Id,
                        Link = m.Link,
                        IsSavedLinks = ci.IsSavedLinks
                    })
                .ToDictionary(k => k.MessageId);
            List<ulong> channelNotFound = new List<ulong>();
            List<ulong> messageNotFound = new List<ulong>();
            using IEnumerator<IGrouping<ulong, MessageCollectionItemJson>> groupEnumerator =
                request.Items.GroupBy(x => x.ChannelId).GetEnumerator();
            while (groupEnumerator.MoveNext())
            {
                using IEnumerator<MessageCollectionItemJson> groupItemEnumerator =
                    groupEnumerator.Current.GetEnumerator();
                DiscordChannel discordChannel = null;
                while (groupItemEnumerator.MoveNext())
                {
                    ulong messageId = groupItemEnumerator.Current.MessageId;
                    if (collectionItems.ContainsKey(messageId))
                    {
                        CollectionItemJoinMessage collectionItem = collectionItems[messageId];

                        response = new UrlsResponse
                        {
                            Next = true,
                            Urls = collectionItem.Link.Split(";").ToList()
                        };
                        _binaryWriter.Write(response.ToString());

                        if (!collectionItem.IsSavedLinks)
                        {
                            Thread.Sleep(_yukoSettings.IntervalBetweenMessageRequests);
                        }
                    }
                    else
                    {
                        try
                        {
                            if (discordChannel == null)
                                discordChannel =
                                    await _discordClient.GetChannelAsync(
                                        groupEnumerator.Current.Key);
                            try
                            {
                                DiscordMessage discordMessage =
                                    await discordChannel.GetMessageAsync(messageId);
                                response = new UrlsResponse { Next = true };
                                response.Urls.AddRange(discordMessage.GetImages(_yukoSettings));
                                _binaryWriter.Write(response.ToString());
                            }
                            catch (NotFoundException)
                            {
                                messageNotFound.Add(groupItemEnumerator.Current.MessageId);
                            }
                            Thread.Sleep(_yukoSettings.IntervalBetweenMessageRequests);
                        }
                        catch (NotFoundException)
                        {
                            channelNotFound.Add(groupEnumerator.Current.Key);
                        }
                    }
                }
            }
            response = new UrlsResponse
            {
                Next = false,
                ErrorMessage = string.Empty
            };
            if (channelNotFound.Count > 0 || messageNotFound.Count > 0)
            {
                if (channelNotFound.Count > 0)
                {
                    response.ErrorMessage +=
                        $"Следующие каналы были не найдены: {string.Join(',', channelNotFound)}.";
                }
                if (messageNotFound.Count > 0)
                {
                    if (response.ErrorMessage.Length > 0)
                    {
                        response.ErrorMessage += '\n';
                    }
                    response.ErrorMessage +=
                        $"Следующие сообщения были не найдены: {string.Join(',', messageNotFound)}.";
                }
            }
            _binaryWriter.Write(response.ToString());
        }

        private async Task<ServerJson> GetServer(DiscordGuild guild)
        {
            DiscordMember isContainsMember = await guild.GetMemberAsync(_currentDbUser.Id);
            if (isContainsMember != null)
            {
                ServerJson serverResponse = new ServerJson
                {
                    Id = guild.Id,
                    Name = guild.Name,
                    IconUri = guild.IconUrl
                };
                IEnumerable<DiscordChannel> channels = await guild.GetChannelsAsync();
                foreach (DiscordChannel channel in channels)
                {
                    if (!channel.IsCategory && channel.Type != ChannelType.Voice)
                    {
                        Permissions userPermission = channel.PermissionsFor(isContainsMember);
                        if (userPermission.HasPermission(
                                Permissions.AccessChannels |
                                Permissions.ReadMessageHistory))
                        {
                            ChannelJson channelResponse = new ChannelJson
                            {
                                Id = channel.Id,
                                Name = channel.Name
                            };
                            serverResponse.Channels.Add(channelResponse);
                        }
                    }
                }
                return serverResponse;
            }
            return null;
        }
        #endregion

        #region Attacments
        private async Task GetAttachment(ExecuteScriptRequest request)
        {
            DiscordChannel discordChannel;
            try
            {
                discordChannel = await _discordClient.GetChannelAsync(request.ChannelId);
            }
            catch (NotFoundException)
            {
                _binaryWriter.Write(new UrlsResponse
                {
                    Next = false,
                    Error = new UrlsErrorJson
                    {
                        Code = ClientErrorCodes.ChannelNotFound,
                        ChannelId = request.ChannelId
                    }
                }.ToString());
                return;
            }

            DiscordMessage discordMessage;
            try
            {
                discordMessage = await _messageRequestQueue.GetMessageAsync(discordChannel, request.MessageId);
            }
            catch (NotFoundException)
            {
                _binaryWriter.Write(new UrlsResponse
                {
                    Next = false,
                    Error = new UrlsErrorJson
                    {
                        Code = ClientErrorCodes.MessageNotFound,
                        MessageId = request.MessageId
                    }
                }.ToString());
                return;
            }

            UrlsResponse response = new UrlsResponse();
            response.Urls.AddRange(discordMessage.GetImages(_yukoSettings));
            _binaryWriter.Write(response.ToString());
        }

        private async Task GetAttachmentsAfter(ExecuteScriptRequest request)
        {
            DiscordChannel discordChannel;
            try
            {
                discordChannel = await _discordClient.GetChannelAsync(request.ChannelId);
            }
            catch (NotFoundException)
            {
                _binaryWriter.Write(new UrlsResponse
                {
                    Next = false,
                    Error = new UrlsErrorJson
                    {
                        Code = ClientErrorCodes.ChannelNotFound,
                        ChannelId = request.ChannelId
                    }
                }.ToString());
                return;
            }

            int limit = _yukoSettings.NumberOfMessagesPerRequest;

            while (request.Count > 0)
            {
                if (request.Count < limit)
                    limit = request.Count;

                IAsyncEnumerable<DiscordMessage> messages =
                    await _messageRequestQueue.GetMessagesAfterAsync(discordChannel, request.MessageId, limit);

                UrlsResponse response = new UrlsResponse();

                bool saveFirst = false;
                int messageCount = 0;
                await foreach (DiscordMessage message in messages)
                {
                    if (!saveFirst)
                    {
                        request.MessageId = message.Id;
                        saveFirst = true;
                    }

                    response.Urls.AddRange(message.GetImages(_yukoSettings));

                    messageCount++;
                }

                request.Count -= messageCount;
                if (messageCount < _yukoSettings.NumberOfMessagesPerRequest)
                    request.Count = 0;

                response.Next = request.Count > 0;

                _binaryWriter.Write(response.ToString());
            }
        }

        private async Task GetAttachmentsBefore(ExecuteScriptRequest request)
        {
            DiscordChannel discordChannel;
            try
            {
                discordChannel = await _discordClient.GetChannelAsync(request.ChannelId);
            }
            catch (NotFoundException)
            {
                _binaryWriter.Write(new UrlsResponse
                {
                    Next = false,
                    Error = new UrlsErrorJson
                    {
                        Code = ClientErrorCodes.ChannelNotFound,
                        ChannelId = request.ChannelId
                    }
                }.ToString());
                return;
            }

            int limit = _yukoSettings.NumberOfMessagesPerRequest;

            while (request.Count > 0)
            {
                if (request.Count < limit)
                    limit = request.Count;

                IAsyncEnumerable<DiscordMessage> messages =
                    await _messageRequestQueue.GetMessagesBeforeAsync(discordChannel, request.MessageId, limit);

                UrlsResponse response = new UrlsResponse();

                bool saveFirst = false;
                int messageCount = 0;
                await foreach (DiscordMessage message in messages)
                {
                    if (!saveFirst)
                    {
                        request.MessageId = message.Id;
                        saveFirst = true;
                    }

                    response.Urls.AddRange(message.GetImages(_yukoSettings));

                    messageCount++;
                }

                request.Count -= messageCount;
                if (messageCount < _yukoSettings.NumberOfMessagesPerRequest)
                    request.Count = 0;

                response.Next = request.Count > 0;

                _binaryWriter.Write(response.ToString());
            }
        }

        private async Task GetAttachments(ExecuteScriptRequest request)
        {
            DiscordChannel discordChannel;
            try
            {
                discordChannel = await _discordClient.GetChannelAsync(request.ChannelId);
            }
            catch (NotFoundException)
            {
                _binaryWriter.Write(new UrlsResponse
                {
                    Next = false,
                    Error = new UrlsErrorJson
                    {
                        Code = ClientErrorCodes.ChannelNotFound,
                        ChannelId = request.ChannelId
                    }
                }.ToString());
                return;
            }

            int limit = _yukoSettings.NumberOfMessagesPerRequest;

            if (request.Count < limit)
                limit = request.Count;

            IAsyncEnumerable<DiscordMessage> messages =
                await _messageRequestQueue.GetMessagesAsync(discordChannel, limit);

            UrlsResponse response = new UrlsResponse();

            int messageCount = 0;
            await foreach (DiscordMessage message in messages)
            {
                response.Urls.AddRange(message.GetImages(_yukoSettings));

                request.MessageId = message.Id;
                messageCount++;
            }

            request.Count -= messageCount;
            if (messageCount < _yukoSettings.NumberOfMessagesPerRequest)
                request.Count = 0;

            response.Next = request.Count > 0;

            _binaryWriter.Write(response.ToString());

            while (request.Count > 0)
            {
                if (request.Count < limit)
                    limit = request.Count;

                messages = await _messageRequestQueue.GetMessagesBeforeAsync(discordChannel, request.MessageId, limit);

                response = new UrlsResponse();

                bool saveFirst = false;
                messageCount = 0;
                await foreach (DiscordMessage message in messages)
                {
                    if (!saveFirst)
                    {
                        request.MessageId = message.Id;
                        saveFirst = true;
                    }

                    response.Urls.AddRange(message.GetImages(_yukoSettings));

                    messageCount++;
                }

                request.Count -= messageCount;
                if (messageCount < _yukoSettings.NumberOfMessagesPerRequest)
                    request.Count = 0;

                response.Next = request.Count > 0;

                _binaryWriter.Write(response.ToString());
            }
        }
        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                _binaryReader?.Dispose();
                _binaryWriter?.Dispose();
                _tcpClient?.Dispose();
                //_dbContext?.Dispose();
                _isDisposed = true;
            }
        }
    }
}