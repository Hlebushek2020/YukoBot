using YukoClientBase.Enums;
using YukoClientBase.Models.Web;
using YukoCollectionsClient.Models.Web.Providers;
using YukoCollectionsClient.Models.Web.Responses;

namespace YukoCollectionsClient.Models.Web
{
    public class WebClient : WebClientBase
    {
        #region Instance
        public static WebClient Current { get; } = new WebClient();
        #endregion

        public MessageCollectionsResponse GetMessageCollections() =>
            Request<MessageCollectionsResponse>(null, RequestType.GetMessageCollections);

        public UrlsProvider GetUrls(MessageCollection messageCollection) =>
            new UrlsProvider(token.ToString(), messageCollection);
    }
}