using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using YukoClient.Exceptions;
using YukoClient.Models.Web.Requests;
using YukoClientBase.Enums;
using YukoClientBase.Models;
using YukoClientBase.Models.Web;
using YukoClientBase.Models.Web.Responses;

namespace YukoClient.Models.Web.Providers
{
    public class ExecuteScriptProvider : IDisposable
    {
        private bool _disposed = false;

        private readonly int _countScripts;
        private int _countSendScripts = 0;

        private readonly TcpClient _client;
        private readonly BinaryReader _clientReader;
        private readonly BinaryWriter _clientWriter;

        public ExecuteScriptProvider(string token, ulong serverId, int scriptsCount)
        {
            _countScripts = scriptsCount;
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
            _clientWriter.Write((int) RequestType.ExecuteScripts);
            _clientWriter.Write(token);
            _clientWriter.Write(new ServerRequest { Id = serverId }.ToString());
        }

        public void ExecuteScript(Script script)
        {
            if (_countSendScripts >= _countScripts)
                throw new ScriptExecutionUnavailableException();

            _countSendScripts++;
            _clientWriter.Write(
                new ExecuteScriptRequest()
                {
                    ChannelId = script.Channel.Id,
                    Mode = script.Mode.Mode,
                    MessageId = script.MessageId,
                    Count = script.Count,
                    HasNext = _countSendScripts < _countScripts
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