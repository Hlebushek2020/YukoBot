using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using YukoClientBase.Enums;
using YukoClientBase.Exceptions;
using YukoClientBase.Models.Web.Requests;
using YukoClientBase.Models.Web.Responses;
using YukoClientBase.Properties;
using YukoClientBase.Views;

namespace YukoClientBase.Models.Web
{
    public class WebClientBase
    {
        public const int SendTimeout = 30000;
        public const int ReceiveTimeout = 30000;

        private string _refreshToken;

        protected string Token;

        public bool TokenAvailability => !string.IsNullOrWhiteSpace(Token);

        protected T Request<T>(Request request, RequestType requestType)
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                tcpClient.SendTimeout = SendTimeout;
                tcpClient.ReceiveTimeout = ReceiveTimeout;
                tcpClient.Connect(Settings.Current.Host, Settings.Current.Port);
                NetworkStream stream = tcpClient.GetStream();
                BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
                BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8);
                // request
                writer.Write((int)requestType);
                writer.Write(Token);
                if (request != null)
                    writer.Write(request.ToString());
                // response
                return JsonConvert.DeserializeObject<T>(reader.ReadString());
            }
        }

        public void RefreshToken()
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                tcpClient.SendTimeout = SendTimeout;
                tcpClient.ReceiveTimeout = ReceiveTimeout;
                tcpClient.Connect(Settings.Current.Host, Settings.Current.Port);
                NetworkStream stream = tcpClient.GetStream();
                BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
                BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8);
                // request
                writer.Write((int)RequestType.RefreshToken);
                writer.Write(_refreshToken);
                // response
                RefreshTokenResponse response =
                    JsonConvert.DeserializeObject<RefreshTokenResponse>(reader.ReadString());

                if (response.Error != null)
                    throw new ClientCodeException(response.Error.Code);

                Token = response.Token;
                _refreshToken = response.RefreshToken;
            }
        }

        public AuthorizationResponse Authorization(string idOrUsername, string password)
        {
            AuthorizationRequest request = new AuthorizationRequest { Login = idOrUsername };

            // hash password
            using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder hashBuilder = new StringBuilder(hashBytes.Length / 2);
                foreach (byte code in hashBytes)
                    hashBuilder.Append(code.ToString("X2"));
                request.Password = hashBuilder.ToString();
            }

            AuthorizationResponse response;
            using (TcpClient tcpClient = new TcpClient())
            {
                tcpClient.SendTimeout = SendTimeout;
                tcpClient.ReceiveTimeout = ReceiveTimeout;
                tcpClient.Connect(Settings.Current.Host, Settings.Current.Port);
                NetworkStream stream = tcpClient.GetStream();
                BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
                BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8);
                // request
                writer.Write((int)RequestType.Authorization);
                writer.Write(request.ToString());
                // response
                response = JsonConvert.DeserializeObject<AuthorizationResponse>(reader.ReadString());

                if (response.Error != null)
                    throw new ClientCodeException(response.Error.Code);

                if (response.TwoFactorAuthentication)
                {
                    tcpClient.ReceiveTimeout = 60000;
                    tcpClient.SendTimeout = 60000;

                    InputWindow inputWindow = new InputWindow();
                    inputWindow.ShowDialog();

                    try
                    {
                        writer.Write(inputWindow.GetInputValue());
                    }
                    catch (Exception)
                    {
                        throw new Exception(Resources.TwoFactorAuthentication_CodeHasExpired);
                    }

                    response = JsonConvert.DeserializeObject<AuthorizationResponse>(reader.ReadString());

                    if (response.Error != null)
                        throw new Exception(Resources.TwoFactorAuthentication_IncorrectCode);
                }
            }

            Token = response.Token;
            _refreshToken = response.RefreshToken;

            return response;
        }
    }
}