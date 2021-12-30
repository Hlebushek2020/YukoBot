using YukoClientBase.Models.Web;

namespace YukoCollectionsClient.Models.Web
{
    public class WebClient : WebClientBase
    {
        #region Instance
        public static WebClient Current { get; } = new WebClient();
        #endregion
    }
}
