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

        public ServerResponse GetServer(ulong serverId)
        {
            try
            {
                return Request<ServerResponse>(new ServerRequest
                {
                    Type = RequestType.GetServer,
                    Token = token,
                    Id = serverId
                });
            }
            catch (Exception ex)
            {
                return new ServerResponse { ErrorMessage = ex.Message };
            }
        }

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