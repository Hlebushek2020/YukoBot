using YukoClient.Models.Web.Errors;
using YukoClient.Models.Web.Providers;
using YukoClient.Models.Web.Requests;
using YukoClient.Models.Web.Responses;
using YukoClientBase.Enums;
using YukoClientBase.Models.Web;
using YukoClientBase.Models.Web.Responses;

namespace YukoClient.Models.Web
{
    public class WebClient : WebClientBase
    {
        #region Instance
        public static WebClient Current { get; } = new WebClient();
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

        public ExecuteScriptProvider ExecuteScripts(
            ulong serverId,
            int scriptCount,
            out Response<ExecuteScriptErrorJson> response)
        {
            ExecuteScriptProvider esp = new ExecuteScriptProvider(Token, serverId, scriptCount, out response);

            if (response.Error.Code != ClientErrorCodes.TokenHasExpired)
                return esp;

            RefreshToken();

            return new ExecuteScriptProvider(Token, serverId, scriptCount, out response);
        }
    }
}