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
using YukoBot.Models.Web;
using YukoBot.Models.Web.Requests;
using YukoBot.Models.Web.Responses;
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
                string requestString = _binaryReader.ReadString();
                _logger.LogDebug($"[{_endPoint}] Request: {requestString}");
                BaseRequest baseRequest = BaseRequest.FromJson(requestString);
                if (baseRequest.Type == RequestType.Authorization)
                {
                    AuthorizationResponse response = await ClientAuthorization(requestString);
                    _binaryWriter.Write(response.ToString());
                }
                else
                {
                    Guid userToken = Guid.Parse(baseRequest.Token);
                    _currentDbUser = await _dbContext.Users.FindAsync(
                                                                      _userTokens.ContainsKey(userToken)
                                                                          ? _userTokens[userToken]
                                                                          : ulong.MinValue);
                    if (_currentDbUser == null || baseRequest.Token == null)
                    {
                        Response baseResponse = new Response
                            { ErrorMessage = "Вы не авторизованы!" };
                        _binaryWriter.Write(baseResponse.ToString());
                    }
                    else
                    {
                        switch (baseRequest.Type)
                        {
                            case RequestType.GetServer:
                                await ClientGetServer(requestString);
                                break;
                            case RequestType.GetServers:
                                await ClientGetServers();
                                break;
                            case RequestType.ExecuteScripts:
                                await ClientExecuteScripts(requestString);
                                break;
                            case RequestType.GetMessageCollections:
                                ClientGetMessageCollections();
                                break;
                            case RequestType.GetUrls:
                                await ClientGetUrls(requestString);
                                break;
                        }
                    }
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
        private async Task<AuthorizationResponse> ClientAuthorization(string json)
        {
            AuthorizationRequest request = AuthorizationRequest.FromJson(json);
            DbUser dbUser = _dbContext.Users.FirstOrDefault(x => x.Nikname == request.Login);
            if (dbUser is null)
            {
                if (ulong.TryParse(request.Login, out ulong id))
                {
                    dbUser = _dbContext.Users.FirstOrDefault(x => x.Id == id);
                }
            }
            if (dbUser == null || !dbUser.Password.Equals(request.Password))
            {
                return new AuthorizationResponse
                {
                    ErrorMessage = "Неверный логин или пароль!",
                    Token = null
                };
            }
            // save db
            Guid userToken = Guid.NewGuid();
            dbUser.LoginTime = DateTime.Now;
            await _dbContext.SaveChangesAsync();
            // add token to dictionary
            _userTokens.AddOrUpdate(userToken, x => dbUser.Id, (x, y) => dbUser.Id);
            // response build
            DiscordUser discordUser = await _discordClient.GetUserAsync(dbUser.Id);
            return new AuthorizationResponse
            {
                Id = discordUser.Id,
                Nikname = discordUser.Username + "#" + discordUser.Discriminator,
                AvatarUri = discordUser.AvatarUrl,
                Token = userToken.ToString()
            };
        }

        private async Task ClientGetServer(string json)
        {
            ServerRequest request = ServerRequest.FromJson(json);
            DiscordGuild guild = await _discordClient.GetGuildAsync(request.Id);
            ServerResponse response = ServerResponse.FromServerWeb(await GetServer(guild));
            _binaryWriter.Write(response.ToString());
        }

        private async Task ClientGetServers()
        {
            ServersResponse response = new ServersResponse();
            foreach (KeyValuePair<ulong, DiscordGuild> guild in _discordClient.Guilds)
            {
                if (guild.Value.Members.Any(x => x.Value.Id == _currentDbUser.Id))
                {
                    ServerWeb server = await GetServer(guild.Value);
                    if (server != null)
                    {
                        response.Servers.Add(server);
                    }
                }
            }
            _binaryWriter.Write(response.ToString());
        }

        private async Task ClientExecuteScripts(string json)
        {
            ServerRequest serverRequest = ServerRequest.FromJson(json);
            DbBan ban = _dbContext.Bans.FirstOrDefault(
                                                       x =>
                                                           x.UserId == _currentDbUser.Id &&
                                                           x.ServerId == serverRequest.Id);
            if (ban == null)
            {
                DiscordGuild guild = await _discordClient.GetGuildAsync(serverRequest.Id);
                DiscordMember isContainsMember = await guild.GetMemberAsync(_currentDbUser.Id);
                if (isContainsMember != null)
                {
                    _binaryWriter.Write(new Response().ToString());
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
                                    await GetAttacmentsBefore(scriptRequest);
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
                            Response response = new Response { ErrorMessage = ex.Message };
                            _binaryWriter.Write(response.ToString());
                            _logger.LogError(ex, $"[{_endPoint}] [{_currentDbUser.Id}] {ex.Message}");
                        }
                    } while (scriptRequest.HasNext);
                }
                else
                {
                    Response response = new Response { ErrorMessage = "Вас нет на этом сервере!" };
                    _binaryWriter.Write(response.ToString());
                }
            }
            else
            {
                string reason = string.IsNullOrEmpty(ban.Reason)
                    ? ""
                    : $" Причина бана: {ban.Reason}.";
                Response response = new Response
                {
                    ErrorMessage =
                        $"Вы забанены на этом сервере, для разбана обратитесь к администратору сервера.{reason}"
                };
                _binaryWriter.Write(response.ToString());
            }
        }

        private void ClientGetMessageCollections()
        {
            IReadOnlyList<DbCollection> dbCollections =
                _dbContext.Collections.Where(x => x.UserId == _currentDbUser.Id).ToList();
            MessageCollectionsResponse response = new MessageCollectionsResponse();
            foreach (DbCollection dbCollection in dbCollections)
            {
                MessageCollectionWeb collection = new MessageCollectionWeb
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
                                         new MessageCollectionItemWeb
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
            using IEnumerator<IGrouping<ulong, MessageCollectionItemWeb>> groupEnumerator =
                request.Items.GroupBy(x => x.ChannelId).GetEnumerator();
            _binaryWriter.Write(new Response().ToString());
            while (groupEnumerator.MoveNext())
            {
                using IEnumerator<MessageCollectionItemWeb> groupItemEnumerator =
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

        private async Task<ServerWeb> GetServer(DiscordGuild guild)
        {
            DiscordMember isContainsMember = await guild.GetMemberAsync(_currentDbUser.Id);
            if (isContainsMember != null)
            {
                ServerWeb serverResponse = new ServerWeb
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
                            ChannelWeb channelResponse = new ChannelWeb
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
            DiscordChannel discordChannel = await _discordClient.GetChannelAsync(request.ChannelId);
            DiscordMessage discordMessage = await discordChannel.GetMessageAsync(request.MessageId);
            UrlsResponse response = new UrlsResponse();
            response.Urls.AddRange(discordMessage.GetImages(_yukoSettings));
            _binaryWriter.Write(response.ToString());
        }

        private async Task GetAttachmentsAfter(ExecuteScriptRequest request)
        {
            DiscordChannel discordChannel = await _discordClient.GetChannelAsync(request.ChannelId);

            int limit = _yukoSettings.NumberOfMessagesPerRequest;

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

                IReadOnlyList<DiscordMessage> messages =
                    await _messageRequestQueue.GetMessagesAfterAsync(discordChannel, request.MessageId, limit);

                if (messages.Count < _yukoSettings.NumberOfMessagesPerRequest)
                    request.Count = 0;

                UrlsResponse response = new UrlsResponse { Next = request.Count > 0 };
                foreach (DiscordMessage message in messages)
                {
                    response.Urls.AddRange(message.GetImages(_yukoSettings));
                }
                _binaryWriter.Write(response.ToString());

                if (messages.Count > 0)
                {
                    request.MessageId = messages.First().Id;
                    //Thread.Sleep(_messageLimitSleepMs);
                }
            }
        }

        private async Task GetAttacmentsBefore(ExecuteScriptRequest request)
        {
            DiscordChannel discordChannel = await _discordClient.GetChannelAsync(request.ChannelId);

            int limit = _yukoSettings.NumberOfMessagesPerRequest;

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

                IReadOnlyList<DiscordMessage> messages =
                    await _messageRequestQueue.GetMessagesBeforeAsync(discordChannel, request.MessageId, limit);

                if (messages.Count < _yukoSettings.NumberOfMessagesPerRequest)
                    request.Count = 0;

                UrlsResponse response = new UrlsResponse { Next = request.Count > 0 };
                foreach (DiscordMessage message in messages)
                {
                    response.Urls.AddRange(message.GetImages(_yukoSettings));
                }
                _binaryWriter.Write(response.ToString());

                if (messages.Count > 0)
                {
                    request.MessageId = messages.First().Id;
                    //Thread.Sleep(_messageLimitSleepMs);
                }
            }
        }

        private async Task GetAttachments(ExecuteScriptRequest request)
        {
            DiscordChannel discordChannel = await _discordClient.GetChannelAsync(request.ChannelId);

            int limit = _yukoSettings.NumberOfMessagesPerRequest;

            if (request.Count >= limit)
            {
                request.Count -= limit;
            }
            else
            {
                limit = request.Count;
                request.Count = 0;
            }

            IReadOnlyList<DiscordMessage> messages = await _messageRequestQueue.GetMessagesAsync(discordChannel, limit);

            UrlsResponse response = new UrlsResponse { Next = request.Count > 0 };
            foreach (DiscordMessage message in messages)
            {
                response.Urls.AddRange(message.GetImages(_yukoSettings));
            }
            _binaryWriter.Write(response.ToString());

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

                //Thread.Sleep(_messageLimitSleepMs);

                messages = await _messageRequestQueue.GetMessagesBeforeAsync(discordChannel, endId, limit);

                if (messages.Count < _yukoSettings.NumberOfMessagesPerRequest)
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
                    response.Urls.AddRange(message.GetImages(_yukoSettings));
                }
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