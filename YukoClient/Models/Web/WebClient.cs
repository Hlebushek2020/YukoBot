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

        public ServerResponse GetServer(ulong serverId) =>
            Request<ServerResponse>(new ServerRequest { Id = serverId }, RequestType.GetServer);

        public ServersResponse GetServers() => Request<ServersResponse>(null, RequestType.GetServers);

        public ExecuteScriptProvider ExecuteScripts(
            ulong serverId,
            int scriptCount,
            out Response<ExecuteScriptErrorJson> response)
        {
            return new ExecuteScriptProvider(Token.ToString(), serverId, scriptCount, out response);
        }
    }
}