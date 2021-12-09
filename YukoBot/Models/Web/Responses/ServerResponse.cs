using System.Collections.Generic;

namespace YukoBot.Models.Web.Responses
{
    public class ServerResponse : Response
    {
        public ulong Id { get; set; }
        public string IconUri { get; set; }
        public string Name { get; set; }
        public List<ChannelResponse> Channels { get; set; } = new List<ChannelResponse>();
    }
}