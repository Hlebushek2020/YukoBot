using System.Collections.Generic;

namespace YukoBot.Models.Json.Responses
{
    public class MessageCollectionsBaseResponse : BaseResponse
    {
        public List<MessageCollectionWeb> MessageCollections { get; set; } = new List<MessageCollectionWeb>();
    }
}