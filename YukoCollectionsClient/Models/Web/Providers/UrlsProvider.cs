using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using YukoClientBase.Enums;
using YukoClientBase.Models;
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
                SendTimeout = WebClient.SendTimeout,
                ReceiveTimeout = WebClient.ReceiveTimeout
            };
            _client.Connect(Settings.Current.Host, Settings.Current.Port);
            NetworkStream networkStream = _client.GetStream();
            _clientReader = new BinaryReader(networkStream, Encoding.UTF8, true);
            _clientWriter = new BinaryWriter(networkStream, Encoding.UTF8, true);
            // request
            UrlsRequest request = new UrlsRequest
            {
                Type = RequestType.GetUrls,
                Token = token,
                Items = messageCollection.Items,
                Id = messageCollection.Id
            };
            _clientWriter.Write(request.ToString());
        }

        public UrlsResponse ReadBlock() =>
            JsonConvert.DeserializeObject<UrlsResponse>(_clientReader.ReadString());

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