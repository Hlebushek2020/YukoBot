﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using YukoClientBase.Enums;
using YukoClientBase.Models;
using YukoCollectionsClient.Models.Web.Requests;
using YukoCollectionsClient.Models.Web.Responses;

namespace YukoCollectionsClient.Models.Web.Providers
{
    public class GetUrlsProvider : IDisposable
    {
        private bool disposed = false;

        private readonly TcpClient client;
        private readonly BinaryReader clientReader;
        private readonly BinaryWriter clientWriter;

        public GetUrlsProvider(string token, IReadOnlyCollection<MessageCollectionItem> messageCollectionItems)
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
            GetUrlsRequest request = new GetUrlsRequest
            {
                Type = RequestType.GetUrls,
                Token = token,
                Items = messageCollectionItems
            };
            clientWriter.Write(request.ToString());
        }

        public GetUrlsResponse ReadBlock()
        {
            return JsonConvert.DeserializeObject<GetUrlsResponse>(clientReader.ReadString());
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