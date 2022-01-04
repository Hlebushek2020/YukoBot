using System.Collections.Generic;

namespace YukoBot.Models.Web.Responses
{
    public class ServersResponse : Response
    {
        public List<ServerWeb> Servers { get; set; }
    }
}
