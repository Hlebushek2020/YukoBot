using System.Collections.Generic;
using System.IO;
using YukoClientBase.Models.Web.Errors;

namespace YukoClientBase.Models.Web.Responses
{
    public class UrlsResponse : Response<BaseErrorJson>
    {
        public bool Next { get; set; }
        public ulong ChannelId { get; set; }
        public ulong MessageId { get; set; }
        public List<string> Urls { get; set; }
    }
}