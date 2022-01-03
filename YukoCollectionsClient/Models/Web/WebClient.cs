using System;
using System.Collections.Generic;
using YukoClientBase.Enums;
using YukoClientBase.Models.Web;
using YukoClientBase.Models.Web.Requests;
using YukoCollectionsClient.Models.Web.Providers;
using YukoCollectionsClient.Models.Web.Responses;

namespace YukoCollectionsClient.Models.Web
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
                    Type = RequestType.GetClientData | RequestType.GetMessageCollections,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return new ClientDataResponse { ErrorMessage = ex.Message };
            }
        }

        public MessageCollectionsResponse GetMessageCollections()
        {
            try
            {
                return Request<MessageCollectionsResponse>(new BaseRequest
                {
                    Type = RequestType.GetMessageCollections,
                    Token = token
                });
            }
            catch (Exception ex)
            {
                return new MessageCollectionsResponse { ErrorMessage = ex.Message };
            }
        }

        public GetUrlsProvider GetUrls(IReadOnlyCollection<MessageCollectionItem> messageCollectionItems)
        {
            return new GetUrlsProvider(token, messageCollectionItems);
        }
    }
}