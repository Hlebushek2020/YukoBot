using System;
using YukoClient.Models.Web.Providers;
using YukoClient.Models.Web.Requests;
using YukoClient.Models.Web.Responses;
using YukoClientBase.Enums;
using YukoClientBase.Models.Web;
using YukoClientBase.Models.Web.Requests;

namespace YukoClient.Models.Web
{
    public class WebClient : WebClientBase
    {
        #region Instance
        public static WebClient Current { get; } = new WebClient();
        #endregion

        public ServerResponse GetServer(ulong serverId) =>
            Request<ServerResponse>(new ServerRequest { Id = serverId }, RequestType.GetServer);

        public ServersResponse GetServers()
        {
            try
            {
                return Request<ServersResponse>(new BaseRequest
                {
                    Type = RequestType.GetServers,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return new ServersResponse { ErrorMessage = ex.Message };
            }
        }

        public ExecuteScriptProvider ExecuteScripts(ulong serverId, int scriptCount)
        {
            return new ExecuteScriptProvider(token, serverId, scriptCount);
        }
    }
}