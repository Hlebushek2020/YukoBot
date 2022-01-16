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
        private bool disposed = false;

        private readonly TcpClient client;
        private readonly BinaryReader clientReader;
        private readonly BinaryWriter clientWriter;

        public UrlsProvider(string token, IReadOnlyCollection<MessageCollectionItem> messageCollectionItems)
        {
            client = new TcpClient
            {
                SendTimeout = WebClient.SendTimeout,
                ReceiveTimeout = WebClient.ReceiveTimeout
            };
            client.Connect(Settings.Current.Host, Settings.Current.Port);
            NetworkStream networkStream = client.GetStream();
            clientReader = new BinaryReader(networkStream, Encoding.UTF8, true);
            clientWriter = new BinaryWriter(networkStream, Encoding.UTF8, true);
            // request
            UrlsRequest request = new UrlsRequest
            {
                Type = RequestType.GetUrls,
                Token = token,
                Items = messageCollectionItems
            };
            clientWriter.Write(request.ToString());
        }

        public UrlsResponse ReadBlock()
        {
            return JsonConvert.DeserializeObject<UrlsResponse>(clientReader.ReadString());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    clientReader.Dispose();
                    clientWriter.Dispose();
                    client.Dispose();
                }
                disposed = true;
            }
        }
    }
}