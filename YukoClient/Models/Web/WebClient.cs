using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using YukoClient.Enums;
using YukoClient.Models.Web.Providers;
using YukoClient.Models.Web.Requests;
using YukoClient.Models.Web.Responses;
using YukoClientBase.Models.Web.Responses;

namespace YukoClient.Models.Web
{
    public static class WebClient
    {
        #region Token
        private static string token;

        public static bool TokenAvailability { get { return !string.IsNullOrEmpty(token); } }
        #endregion

        public static AuthorizationResponse Authorization(string idOrNikname, string password)
        {
            AuthorizationResponse response = null;
            TcpClient tcpClient = new TcpClient
            {
                SendTimeout = 30000,
                ReceiveTimeout = 30000
            };
            try
            {
                tcpClient.Connect(Settings.Current.Host, Settings.Current.Port);
                NetworkStream stream = tcpClient.GetStream();
                BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
                BinaryWriter writter = new BinaryWriter(stream, Encoding.UTF8);
                AuthorizationRequest request = new AuthorizationRequest
                {
                    Login = idOrNikname,
                    Type = RequestType.Authorization
                };
                // hash password
                using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
                {
                    byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                    StringBuilder hashBuilder = new StringBuilder(hashBytes.Length / 2);
                    foreach (byte code in hashBytes)
                    {
                        hashBuilder.Append(code.ToString("X2"));
                    }
                    request.Password = hashBuilder.ToString();
                }
                // request
                writter.Write(request.ToString());
                // response
                response = AuthorizationResponse.FromJson(reader.ReadString());
                token = response.Token;
            }
            catch (Exception ex)
            {
                response = new AuthorizationResponse
                {
                    ErrorMessage = ex.Message
                };
            }
            finally
            {
                tcpClient.Dispose();
            }
            return response;
        }

        public static ClientDataResponse GetClientData()
        {
            ClientDataResponse response = null;
            TcpClient tcpClient = new TcpClient
            {
                SendTimeout = 30000,
                ReceiveTimeout = 30000
            };
            try
            {
                tcpClient.Connect(Settings.Current.Host, Settings.Current.Port);
                NetworkStream stream = tcpClient.GetStream();
                BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
                BinaryWriter writter = new BinaryWriter(stream, Encoding.UTF8);
                // request
                BaseRequest request = new BaseRequest
                {
                    Type = RequestType.GetClientData,
                    Token = token
                };
                writter.Write(request.ToString());
                // response
                response = ClientDataResponse.FromJson(reader.ReadString());
            }
            catch (Exception ex)
            {
                response = new ClientDataResponse
                {
                    ErrorMessage = ex.Message
                };
            }
            finally
            {
                tcpClient.Dispose();
            }
            return response;
        }

        public static ServerResponse UpdateServer(ulong serverId)
        {
            ServerResponse response = null;
            TcpClient tcpClient = new TcpClient
            {
                SendTimeout = 30000,
                ReceiveTimeout = 30000
            };
            try
            {
                tcpClient.Connect(Settings.Current.Host, Settings.Current.Port);
                NetworkStream stream = tcpClient.GetStream();
                BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
                BinaryWriter writter = new BinaryWriter(stream, Encoding.UTF8);
                // request
                ServerRequest serverRequest = new ServerRequest
                {
                    Type = RequestType.UpdateServer,
                    Token = token,
                    Id = serverId
                };
                writter.Write(serverRequest.ToString());
                // response
                response = ServerResponse.FromJson(reader.ReadString());
            }
            catch (Exception ex)
            {
                response = new ServerResponse
                {
                    ErrorMessage = ex.Message
                };
            }
            finally
            {
                tcpClient.Dispose();
            }
            return response;
        }

        public static ServerListResponse UpdateServerList()
        {
            ServerListResponse response = null;
            TcpClient tcpClient = new TcpClient
            {
                SendTimeout = 30000,
                ReceiveTimeout = 30000
            };
            try
            {
                tcpClient.Connect(Settings.Current.Host, Settings.Current.Port);
                NetworkStream stream = tcpClient.GetStream();
                BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
                BinaryWriter writter = new BinaryWriter(stream, Encoding.UTF8);
                // request
                BaseRequest request = new BaseRequest
                {
                    Type = RequestType.UpdateServerList,
                    Token = token
                };
                writter.Write(request.ToString());
                // response
                response = ServerListResponse.FromJson(reader.ReadString());
            }
            catch (Exception ex)
            {
                response = new ServerListResponse
                {
                    ErrorMessage = ex.Message
                };
            }
            finally
            {
                tcpClient.Dispose();
            }
            return response;
        }

        public static ExecuteScriptProvider ExecuteScripts(ulong serverId, int scriptCount)
        {
            return new ExecuteScriptProvider(token, serverId, scriptCount);
        }
    }
}