using System.Collections.Generic;
using YukoClientBase.Models.Web.Responses;

namespace YukoClient.Models.Web.Responses
{
    public class ServerListResponse : Response
    {
        public List<Server> Servers { get; set; }
    }
}
