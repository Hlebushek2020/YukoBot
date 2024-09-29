using System.Collections.Generic;

namespace YukoBot.Models.Json
{
    public class MessageCollectionJson
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public List<MessageCollectionItemJson> Items { get; set; } = new List<MessageCollectionItemJson>();
    }
}