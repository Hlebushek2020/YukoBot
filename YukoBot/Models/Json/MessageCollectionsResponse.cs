using System.Collections.Generic;

namespace YukoBot.Models.Json.Responses
{
    public class MessageCollectionsResponse : Response
    {
        public List<MessageCollectionWeb> MessageCollections { get; set; } = new List<MessageCollectionWeb>();
    }
}