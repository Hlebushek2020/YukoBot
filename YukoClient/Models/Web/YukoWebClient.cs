using YukoClient.Models.Web.Providers;
using YukoClient.Models.Web.Requests;
using YukoClient.Models.Web.Responses;
using YukoClientBase.Enums;
using YukoClientBase.Exceptions;
using YukoClientBase.Models.Web;

namespace YukoClient.Models.Web
{
    public class YukoWebClient : YukoWebClientBase
    {
        #region Instance
        public static YukoWebClient Current { get; } = new YukoWebClient();
        #endregion

        public ServerResponse GetServer(ulong serverId)
        {
            ServerResponse sr = Request<ServerResponse>(new ServerRequest { Id = serverId }, RequestType.GetServer);

            if (sr.Error.Code != ClientErrorCodes.TokenHasExpired)
                return sr;

            RefreshToken();

            return Request<ServerResponse>(new ServerRequest { Id = serverId }, RequestType.GetServer);
        }

        public ServersResponse GetServers()
        {
            ServersResponse sr = Request<ServersResponse>(null, RequestType.GetServers);

            if (sr.Error?.Code != ClientErrorCodes.TokenHasExpired)
                return sr;

            RefreshToken();

            return Request<ServersResponse>(null, RequestType.GetServers);
        }

        public ExecuteScriptProvider ExecuteScripts(ulong serverId, int scriptCount)
        {
            try
            {
                return new ExecuteScriptProvider(Token, serverId, scriptCount);
            }
            catch (ClientCodeException ex)
            {
                if (ex.ClientErrorCode != ClientErrorCodes.TokenHasExpired)
                    throw;

                RefreshToken();

                return new ExecuteScriptProvider(Token, serverId, scriptCount);
            }
        }
    }
}