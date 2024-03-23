using System.Collections.Generic;

namespace YukoBot.Models.Json
{
    public class ServerJson
    {
        public ulong Id { get; set; }
        public string IconUri { get; set; }
        public string Name { get; set; }
        public List<ChannelJson> Channels { get; set; } = new List<ChannelJson>();
    }
}