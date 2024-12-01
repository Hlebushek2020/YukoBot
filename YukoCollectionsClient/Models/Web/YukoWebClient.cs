using YukoClientBase.Enums;
using YukoClientBase.Exceptions;
using YukoClientBase.Models.Web;
using YukoCollectionsClient.Models.Web.Providers;
using YukoCollectionsClient.Models.Web.Responses;

namespace YukoCollectionsClient.Models.Web
{
    public class YukoWebClient : YukoWebClientBase
    {
        #region Instance
        public static YukoWebClient Current { get; } = new YukoWebClient();
        #endregion

        public MessageCollectionsResponse GetMessageCollections()
        {
            MessageCollectionsResponse response = Request<MessageCollectionsResponse>(
                null,
                RequestType.GetMessageCollections);

            if (response.Error == null || response.Error.Code != ClientErrorCodes.TokenHasExpired)
                return response;

            RefreshToken();

            return Request<MessageCollectionsResponse>(null, RequestType.GetMessageCollections);
        }

        public UrlsProvider GetUrls(MessageCollection messageCollection)
        {
            try
            {
                return new UrlsProvider(Token, messageCollection);
            }
            catch (ClientCodeException ex)
            {
                if (ex.ClientErrorCode != ClientErrorCodes.TokenHasExpired)
                    throw;

                RefreshToken();

                return new UrlsProvider(Token, messageCollection);
            }
        }
    }
}