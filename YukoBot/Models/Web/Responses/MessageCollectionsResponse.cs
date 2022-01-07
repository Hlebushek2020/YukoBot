using System.Collections.Generic;

namespace YukoBot.Models.Web.Responses
{
    public class MessageCollectionsResponse : Response
    {
        public List<MessageCollectionWeb> MessageCollections { get; set; } = new List<MessageCollectionWeb>();
    }
}