using System.Collections.Generic;

namespace YukoBot.Models.Json.Requests
{
    public class UrlsRequest : Request<UrlsRequest>
    {
        public ulong Id { get; set; }
        public List<MessageCollectionItemJson> Items { get; set; }
    }
}