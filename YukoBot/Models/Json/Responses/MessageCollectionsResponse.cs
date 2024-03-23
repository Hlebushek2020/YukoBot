using System.Collections.Generic;
using YukoBot.Models.Json.Errors;

namespace YukoBot.Models.Json.Responses
{
    public class MessageCollectionsResponse : Response<BaseErrorJson>
    {
        public List<MessageCollectionJson> MessageCollections { get; set; } = new List<MessageCollectionJson>();
    }
}