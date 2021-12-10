using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using YukoBot.Enums;
using YukoBot.Models.Database;
using YukoBot.Models.Web.Requests;
using YukoBot.Models.Web.Responses;

namespace YukoBot
{
    public partial class YukoBot : IDisposable
    {
        #region Tcp Client
        private async void TcpClientProcessing(object obj)
        {
            TcpClient tcpClient = (TcpClient)obj;
            string endPoint = tcpClient.Client.RemoteEndPoint.ToString();
            Console.WriteLine($"[Server] [{endPoint}] Connected");
            BinaryReader binaryReader = null;
            BinaryWriter binaryWriter = null;
            try
            {
                NetworkStream networkStream = tcpClient.GetStream();
                binaryReader = new BinaryReader(networkStream, Encoding.UTF8, true);
                binaryWriter = new BinaryWriter(networkStream, Encoding.UTF8, true);
                string requestString = binaryReader.ReadString();
                Console.WriteLine($"[Server] [{endPoint}] Request: {requestString}");
                BaseRequest baseRequest = BaseRequest.FromJson(requestString);
                if (baseRequest.Type == RequestType.Authorization)
                {
                    TC_Authorization(requestString, binaryWriter);
                }
                else
                {
                    YukoDbContext db = new YukoDbContext();
                    DbUser dbUser = db.Users.Where(x => x.Token == baseRequest.Token).FirstOrDefault();
                    if (dbUser == null || baseRequest.Token == null)
                    {
                        Response baseResponse = new Response
                        {
                            ErrorMessage = "Вы не авторизованы!",
                        };
                        binaryWriter.Write(baseResponse.ToString());
                    }
                    else
                    {

                        switch (baseRequest.Type)
                        {
                            case RequestType.GetClientData:
                                await TC_GetClientData(dbUser, binaryWriter);
                                break;
                            case RequestType.UpdateServer:
                                await TC_UpdateServer(requestString, dbUser, binaryWriter);
                                break;
                            case RequestType.UpdateServerList:
                                await TC_UpdateServerList(dbUser, binaryWriter);
                                break;
                            case RequestType.ExecuteScripts:
                                await TC_ExecuteScripts(requestString, db, dbUser, binaryReader, binaryWriter);
                                break;
                            case RequestType.UpdateAvatar:
                                await TC_UpdateAvatar(dbUser, binaryWriter);
                                break;
                            default:
                                Response baseResponse = new Response
                                {
                                    ErrorMessage = "Некорректный запрос",
                                };
                                binaryWriter.Write(baseResponse.ToString());
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] [{ex.GetType()}] {ex.Message}");
            }
            finally
            {
                binaryReader?.Dispose();
                binaryWriter?.Dispose();
                tcpClient?.Dispose();
                Console.WriteLine($"[Server] [{endPoint}] Disconnected");
            }
        }

        private void TC_Authorization(string json, BinaryWriter writer)
        {
            AuthorizationRequest request = AuthorizationRequest.FromJson(json);
            YukoDbContext db = new YukoDbContext();
            DbUser dbUser = db.Users.Where(x => x.Nikname == request.Login).FirstOrDefault();
            if (dbUser is null)
            {
                if (ulong.TryParse(request.Login, out ulong id))
                {
                    dbUser = db.Users.Where(x => x.Id == id).FirstOrDefault();
                }
            }
            if (dbUser == null || !dbUser.Password.Equals(request.Password))
            {
                AuthorizationResponse response = new AuthorizationResponse
                {
                    ErrorMessage = "Неверный логин или пароль!",
                    Token = null
                };
                writer.Write(response.ToString());
            }
            else
            {
                AuthorizationResponse response = new AuthorizationResponse
                {
                    Token = Guid.NewGuid().ToString()
                };
                writer.Write(response.ToString());
                // save db
                dbUser.Token = response.Token;
                dbUser.LoginTime = DateTime.Now;
                db.SaveChanges();
            }
        }

        private async Task TC_UpdateAvatar(DbUser dbUser, BinaryWriter writer)
        {
            DiscordUser user = await discordClient.GetUserAsync(dbUser.Id);
            UpdateAvatarResponse response = new UpdateAvatarResponse
            {
                AvatarUri = user.AvatarUrl
            };
            writer.Write(response.ToString());
        }

        private async Task TC_GetClientData(DbUser dbUser, BinaryWriter writer)
        {
            DiscordUser user = await discordClient.GetUserAsync(dbUser.Id);
            ClientDataResponse response = new ClientDataResponse
            {
                AvatarUri = user.AvatarUrl,
                Nikname = dbUser.Nikname,
                Id = dbUser.Id
            };
            foreach (KeyValuePair<ulong, DiscordGuild> guild in discordClient.Guilds)
            {
                ServerResponse serverResponse = await TC_S_GetServer(dbUser, guild.Value);
                if (serverResponse != null)
                {
                    response.Servers.Add(serverResponse);
                }
            }
            writer.Write(response.ToString());
        }

        private async Task TC_UpdateServer(string json, DbUser dbUser, BinaryWriter writer)
        {
            ServerRequest request = ServerRequest.FromJson(json);
            DiscordGuild guild = await discordClient.GetGuildAsync(request.Id);
            DiscordMember isContainsMember = await guild.GetMemberAsync(dbUser.Id);
            ServerResponse response = new ServerResponse
            {
                ErrorMessage = "Вас нет на этом сервере!"
            };
            ServerResponse serverResponse = await TC_S_GetServer(dbUser, guild);
            if (serverResponse != null)
            {
                response = serverResponse;
            }
            writer.Write(response.ToString());
        }

        private async Task TC_UpdateServerList(DbUser dbUser, BinaryWriter writer)
        {
            ServerListResponse response = new ServerListResponse();
            foreach (KeyValuePair<ulong, DiscordGuild> guild in discordClient.Guilds)
            {
                ServerResponse serverResponse = await TC_S_GetServer(dbUser, guild.Value);
                if (serverResponse != null)
                {
                    response.Servers.Add(serverResponse);
                }
            }
            writer.Write(response.ToString());
        }

        private async Task TC_ExecuteScripts(string json, YukoDbContext db, DbUser dbUser, BinaryReader reader, BinaryWriter writer)
        {
            ServerRequest serverRequest = ServerRequest.FromJson(json);
            DbBan ban = db.Bans.Where(x => x.UserId == dbUser.Id && x.ServerId == serverRequest.Id).FirstOrDefault();
            if (ban == null)
            {
                DiscordGuild guild = await discordClient.GetGuildAsync(serverRequest.Id);
                DiscordMember isContainsMember = await guild.GetMemberAsync(dbUser.Id);
                if (isContainsMember != null)
                {
                    writer.Write(new Response().ToString());
                    ExecuteScriptRequest scriptRequest;
                    do
                    {
                        scriptRequest = ExecuteScriptRequest.FromJson(reader.ReadString());
                        try
                        {
                            switch (scriptRequest.Mode)
                            {
                                case ScriptMode.One:
                                    await GetAttachment(dbUser, scriptRequest, writer);
                                    break;
                                case ScriptMode.After:
                                    await GetAttachmentsAfter(dbUser, scriptRequest, writer);
                                    break;
                                case ScriptMode.Before:
                                    await GetAttacmentsBefore(dbUser, scriptRequest, writer);
                                    break;
                                case ScriptMode.End:
                                    await GetAttachments(dbUser, scriptRequest, writer);
                                    break;
                                case ScriptMode.All:
                                    scriptRequest.Count = int.MaxValue;
                                    await GetAttachments(dbUser, scriptRequest, writer);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            Response response = new Response
                            {
                                ErrorMessage = ex.Message
                            };
                            writer.Write(response.ToString());
                            Console.WriteLine($"[ERROR] [{ex.GetType()}] {ex.Message}");
                        }
                    } while (scriptRequest.HasNext);

                }
                else
                {
                    Response response = new Response
                    {
                        ErrorMessage = "Вас нет на этом сервере!"
                    };
                    writer.Write(response.ToString());
                }
            }
            else
            {
                string reason = string.IsNullOrEmpty(ban.Reason) ? "" : $" Причина бана: {ban.Reason}.";
                Response response = new Response
                {
                    ErrorMessage = $"Вы забанены на этом сервере, для разбана обратитесь к админу сервера.{reason}"
                };
                writer.Write(response.ToString());
            }
        }
        #endregion

        #region Tcp Sub
        private async Task<ServerResponse> TC_S_GetServer(DbUser dbUser, DiscordGuild guild)
        {
            DiscordMember isContainsMember = await guild.GetMemberAsync(dbUser.Id);
            if (isContainsMember != null)
            {
                ServerResponse serverResponse = new ServerResponse
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
                        if (userPermission.HasPermission(Permissions.AccessChannels | Permissions.ReadMessageHistory))
                        {
                            ChannelResponse channelResponse = new ChannelResponse
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
    }
}