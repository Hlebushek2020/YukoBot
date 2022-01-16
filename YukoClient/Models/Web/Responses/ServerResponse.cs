using System.Collections.Generic;
using YukoClientBase.Models.Web.Responses;

namespace YukoClient.Models.Web.Responses
{
    public class ServerResponse : Response
    {
        public string IconUri { get; set; }
        public string Name { get; set; }
        public List<Channel> Channels { get; set; }
    }
}
