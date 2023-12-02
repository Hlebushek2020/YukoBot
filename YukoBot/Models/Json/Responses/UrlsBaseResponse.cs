using System.Collections.Generic;

namespace YukoBot.Models.Json.Responses
{
    public class UrlsBaseResponse : BaseResponse
    {
        public bool Next { get; set; } = false;
        public List<string> Urls { get; set; } = new List<string>();
    }
}