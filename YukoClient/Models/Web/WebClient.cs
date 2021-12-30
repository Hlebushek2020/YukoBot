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

        public ClientDataResponse GetClientData()
        {
            try
            {
                return Request<ClientDataResponse>(new BaseRequest
                {
                    Type = RequestType.GetClientData,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return new ClientDataResponse { ErrorMessage = ex.Message };
            }
        }

        public ServerResponse UpdateServer(ulong serverId)
        {
            try
            {
                return Request<ServerResponse>(new ServerRequest
                {
                    Type = RequestType.UpdateServer,
                    Token = token,
                    Id = serverId
                });
            }
            catch (Exception ex)
            {
                return new ServerResponse { ErrorMessage = ex.Message };
            }
        }

        public ServerListResponse UpdateServerList()
        {
            try
            {
                return Request<ServerListResponse>(new BaseRequest
                {
                    Type = RequestType.UpdateServerList,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return new ServerListResponse { ErrorMessage = ex.Message };
            }
        }

        public ExecuteScriptProvider ExecuteScripts(ulong serverId, int scriptCount)
        {
            return new ExecuteScriptProvider(token, serverId, scriptCount);
        }
    }
}