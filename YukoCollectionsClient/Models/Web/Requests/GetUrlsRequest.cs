using System.Collections.Generic;
using YukoClientBase.Models.Web.Requests;

namespace YukoCollectionsClient.Models.Web.Requests
{
    public class GetUrlsRequest : BaseRequest
    {
        public IReadOnlyCollection<MessageCollectionItem> Items { get; set; }
    }
}
