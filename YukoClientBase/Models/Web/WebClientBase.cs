﻿using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using YukoClientBase.Enums;
using YukoClientBase.Models.Web.Requests;
using YukoClientBase.Models.Web.Responses;

namespace YukoClientBase.Models.Web
{
    public class WebClientBase
    {
        public const int SendTimeout = 30000;
        public const int ReceiveTimeout = 30000;

        #region Token
        protected string token;

        public bool TokenAvailability { get { return !string.IsNullOrEmpty(token); } }
        #endregion

        protected T Request<T>(Request request)
        {
            using (TcpClient tcpClient = new TcpClient
            {
                SendTimeout = SendTimeout,
                ReceiveTimeout = ReceiveTimeout
            })
            {
                tcpClient.Connect(Settings.Current.Host, Settings.Current.Port);
                NetworkStream stream = tcpClient.GetStream();
                BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
                BinaryWriter writter = new BinaryWriter(stream, Encoding.UTF8);
                // request
                writter.Write(request.ToString());
                // response
                return JsonConvert.DeserializeObject<T>(reader.ReadString());
            }
        }

        public AuthorizationResponse Authorization(string idOrNikname, string password)
        {
            AuthorizationResponse response;
            try
            {
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
                response = Request<AuthorizationResponse>(request);
                token = response.Token;
            }
            catch (Exception ex)
            {
                response = new AuthorizationResponse
                {
                    ErrorMessage = ex.Message
                };
            }
            return response;
        }
    }
}