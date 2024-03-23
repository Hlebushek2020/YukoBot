using System.Collections.Generic;
using YukoClientBase.Models.Web.Errors;
using YukoClientBase.Models.Web.Responses;

namespace YukoCollectionsClient.Models.Web.Responses
{
    public class MessageCollectionsResponse : Response<BaseErrorJson>
    {
        public List<MessageCollection> MessageCollections { get; set; }
    }
}