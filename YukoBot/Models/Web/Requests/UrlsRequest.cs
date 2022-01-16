using System.Collections.Generic;

namespace YukoBot.Models.Web.Requests
{
    public class UrlsRequest : BaseRequest<UrlsRequest>
    {
        public List<MessageCollectionItemWeb> Items { get; set; }
    }
}
