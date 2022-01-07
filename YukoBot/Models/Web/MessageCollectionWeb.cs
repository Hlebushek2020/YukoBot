using System.Collections.Generic;

namespace YukoBot.Models.Web
{
    public class MessageCollectionWeb
    {
        public string Name { get; set; }
        public List<MessageCollectionItemWeb> Items { get; set; } = new List<MessageCollectionItemWeb>();
    }
}