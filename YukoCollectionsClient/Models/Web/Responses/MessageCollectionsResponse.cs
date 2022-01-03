using System.Collections.Generic;
using YukoClientBase.Models.Web.Responses;

namespace YukoCollectionsClient.Models.Web.Responses
{
    public class MessageCollectionsResponse : Response
    {
        public List<MessageCollection> MessageCollections { get; set; }
    }
}