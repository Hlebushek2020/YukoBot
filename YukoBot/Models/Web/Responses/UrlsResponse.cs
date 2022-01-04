using System.Collections.Generic;

namespace YukoBot.Models.Web.Responses
{
    public class UrlsResponse : Response
    {
        public List<string> Urls { get; set; }
        public bool Next { get; set; }
    }
}
