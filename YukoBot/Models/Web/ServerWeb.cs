using System.Collections.Generic;

namespace YukoBot.Models.Web
{
    public class ServerWeb
    {
        public ulong Id { get; set; }
        public string IconUri { get; set; }
        public string Name { get; set; }
        public List<ChannelWeb> Channels { get; set; }
    }
}