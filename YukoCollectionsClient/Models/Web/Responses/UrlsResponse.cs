using System.Collections.Generic;
using YukoClientBase.Models.Web.Responses;

namespace YukoCollectionsClient.Models.Web.Responses
{
    public class UrlsResponse : Response
    {
        public List<string> Urls { get; set; }
        public bool Next { get; set; }
    }
}
