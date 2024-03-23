using System.Collections.Generic;
using YukoClientBase.Models.Web.Errors;
using YukoClientBase.Models.Web.Responses;

namespace YukoClient.Models.Web.Responses
{
    public class ServersResponse : Response<BaseErrorJson>
    {
        public List<Server> Servers { get; set; }
    }
}