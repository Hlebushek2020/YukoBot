using System.Collections.Generic;

namespace YukoBot.Models.Web.Responses
{
    public class ServerListResponse : Response
    {
        public List<ServerResponse> Servers { get; set; } = new List<ServerResponse>();
    }
}
