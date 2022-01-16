using System.Collections.Generic;
using YukoClientBase.Models.Web.Responses;

namespace YukoClient.Models.Web.Responses
{
    public class ServersResponse : Response
    {
        public List<Server> Servers { get; set; }
    }
}
