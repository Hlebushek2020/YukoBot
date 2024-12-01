using YukoClientBase.Enums;
using YukoClientBase.Models.Web;
using YukoClientBase.Models.Web.Errors;
using YukoClientBase.Models.Web.Responses;
using YukoCollectionsClient.Models.Web.Providers;
using YukoCollectionsClient.Models.Web.Responses;

namespace YukoCollectionsClient.Models.Web
{
    public class YukoWebClient : YukoWebClientBase
    {
        #region Instance
        public static YukoWebClient Current { get; } = new YukoWebClient();
        #endregion

        public MessageCollectionsResponse GetMessageCollections() =>
            Request<MessageCollectionsResponse>(null, RequestType.GetMessageCollections);

        public UrlsProvider GetUrls(MessageCollection messageCollection, out Response<BaseErrorJson> response) =>
            new UrlsProvider(Token.ToString(), messageCollection, out response);
    }
}