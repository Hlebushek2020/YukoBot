using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
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
        protected Guid? token;

        public bool TokenAvailability => token.HasValue;
        #endregion

        protected T Request<T>(Request request, RequestType requestType)
        {
            using (TcpClient tcpClient = new TcpClient())
            {
                tcpClient.SendTimeout = SendTimeout;
                tcpClient.ReceiveTimeout = ReceiveTimeout;
                tcpClient.Connect(Settings.Current.Host, Settings.Current.Port);
                NetworkStream stream = tcpClient.GetStream();
                BinaryReader reader = new BinaryReader(stream, Encoding.UTF8);
                BinaryWriter writter = new BinaryWriter(stream, Encoding.UTF8);
                // request
                writter.Write((int) requestType);
                if (requestType != RequestType.Authorization)
                    writter.Write(token.ToString());
                writter.Write(request.ToString());
                // response
                return JsonConvert.DeserializeObject<T>(reader.ReadString());
            }
        }

        public AuthorizationResponse Authorization(string idOrNikname, string password)
        {
            AuthorizationRequest request = new AuthorizationRequest { Login = idOrNikname };
            // hash password
            using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder hashBuilder = new StringBuilder(hashBytes.Length / 2);
                foreach (byte code in hashBytes)
                    hashBuilder.Append(code.ToString("X2"));
                request.Password = hashBuilder.ToString();
            }
            AuthorizationResponse response = Request<AuthorizationResponse>(request, RequestType.Authorization);
            token = response.Token;
            return response;
        }
    }
}