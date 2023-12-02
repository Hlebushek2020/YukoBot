using System.Collections.Generic;

namespace YukoBot.Models.Json
{
    public class MessageCollectionWeb
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public List<MessageCollectionItemWeb> Items { get; set; } = new List<MessageCollectionItemWeb>();
    }
}