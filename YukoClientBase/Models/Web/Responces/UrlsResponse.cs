using System.Collections.Generic;

namespace YukoClientBase.Models.Web.Responses
{
    public class UrlsResponse : Response
    {
        public bool Next { get; set; }
        public List<string> Urls { get; set; }
    }
}
