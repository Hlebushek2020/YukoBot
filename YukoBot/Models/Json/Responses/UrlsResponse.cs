using System.Collections.Generic;

namespace YukoBot.Models.Json.Responses
{
    public class UrlsResponse : Response<UrlsErrorJson>
    {
        public bool Next { get; set; }
        public List<string> Urls { get; set; } = new List<string>();
    }
}