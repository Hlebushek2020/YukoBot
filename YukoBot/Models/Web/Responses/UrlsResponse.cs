using System.Collections.Generic;

namespace YukoBot.Models.Web.Responses
{
    public class UrlsResponse : Response
    {
        public bool Next { get; set; } = false;
        public List<string> Urls { get; set; } = new List<string>();
    }
}