using System.Collections.Generic;

namespace YukoBot.Models.Web.Requests
{
    public class UrlsRequest : BaseRequest<UrlsRequest>
    {
        public ulong Id { get; set; }
        public List<MessageCollectionItemWeb> Items { get; set; }
    }
}