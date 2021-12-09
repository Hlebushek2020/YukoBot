using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using YukoClient.Enums;
using YukoClient.Exceptions;
using YukoClient.Models.Web.Requests;
using YukoClient.Models.Web.Responses;

namespace YukoClient.Models.Web.Providers
{
    public class ExecuteScriptProvider : IDisposable
    {
        private bool disposed = false;

        private int countScripts;
        private int countSendScripts = 0;

        private readonly TcpClient client;
        private readonly BinaryReader clientReader;
        private readonly BinaryWriter clientWriter;

        public ExecuteScriptProvider(string token, ulong serverId, int scriptsCount)
        {
            countScripts = scriptsCount;
            client = new TcpClient
            {
                SendTimeout = 30000,
                ReceiveTimeout = 30000
            };
            client.Connect(Settings.Current.Host, Settings.Current.Port);
            NetworkStream networkStream = client.GetStream();
            clientReader = new BinaryReader(networkStream, Encoding.UTF8, true);
            clientWriter = new BinaryWriter(networkStream, Encoding.UTF8, true);
            // request
            ServerRequest request = new ServerRequest
            {
                Type = RequestType.ExecuteScripts,
                Token = token,
                Id = serverId
            };
            clientWriter.Write(request.ToString());
        }

        public void ExecuteScript(Script script)
        {
            if (countSendScripts >= countScripts)
            {
                throw new ScriptExecutionUnavailableException();
            }
            countSendScripts++;
            ExecuteScriptRequest request = new ExecuteScriptRequest()
            {
                ChannelId = script.Channel.Id,
                Mode = script.Mode.Mode,
                MessageId = script.MessageId,
                Count = script.Count,
                HasNext = countSendScripts < countScripts
            };
            clientWriter.Write(request.ToString());
        }

        public ExecuteScriptResponse ReadBlock()
        {
            return ExecuteScriptResponse.FromJson(clientReader.ReadString());
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