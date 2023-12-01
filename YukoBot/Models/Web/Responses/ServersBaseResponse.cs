using System.Collections.Generic;

namespace YukoBot.Models.Web.Responses
{
    public class ServersBaseResponse : BaseResponse
    {
        public List<ServerWeb> Servers { get; set; } = new List<ServerWeb>();
    }
}