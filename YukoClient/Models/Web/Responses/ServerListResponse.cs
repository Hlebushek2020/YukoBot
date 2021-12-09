using System.Collections.Generic;

namespace YukoClient.Models.Web.Responses
{
    public class ServerListResponse : Response<ServerListResponse>
    {
        public List<Server> Servers { get; set; }
    }
}
