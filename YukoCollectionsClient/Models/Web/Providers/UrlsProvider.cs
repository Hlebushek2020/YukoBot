using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using YukoClientBase.Enums;
using YukoClientBase.Models;
using YukoClientBase.Models.Web;
using YukoClientBase.Models.Web.Responses;
using YukoCollectionsClient.Models.Web.Requests;

namespace YukoCollectionsClient.Models.Web.Providers
{
    public class UrlsProvider : IDisposable
    {
        private bool _disposed = false;

        private readonly TcpClient _client;
        private readonly BinaryReader _clientReader;
        private readonly BinaryWriter _clientWriter;

        public UrlsProvider(string token, MessageCollection messageCollection)
        {
            _client = new TcpClient
            {
                SendTimeout = WebClientBase.SendTimeout,
                ReceiveTimeout = WebClientBase.ReceiveTimeout
            };
            _client.Connect(Settings.Current.Host, Settings.Current.Port);
            NetworkStream networkStream = _client.GetStream();
            _clientReader = new BinaryReader(networkStream, Encoding.UTF8, true);
            _clientWriter = new BinaryWriter(networkStream, Encoding.UTF8, true);
            // request
            _clientWriter.Write((int) RequestType.GetUrls);
            _clientWriter.Write(token);
            _clientWriter.Write(
                new UrlsRequest
                {
                    Items = messageCollection.Items,
                    Id = messageCollection.Id
                }.ToString());
        }

        public UrlsResponse ReadBlock() => JsonConvert.DeserializeObject<UrlsResponse>(_clientReader.ReadString());

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _clientReader.Dispose();
                    _clientWriter.Dispose();
                    _client.Dispose();
                }
                _disposed = true;
            }
        }
    }
}