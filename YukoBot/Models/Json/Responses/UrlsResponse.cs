using System.Collections.Generic;
using YukoBot.Models.Json.Errors;

namespace YukoBot.Models.Json.Responses
{
    public class UrlsResponse : Response<BaseErrorJson>
    {
        public bool Next { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public List<string> Urls { get; set; } = new List<string>();
    }
}