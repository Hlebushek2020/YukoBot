using System.Collections.Generic;
using YukoClientBase.Models.Web.Errors;
using YukoClientBase.Models.Web.Responses;

namespace YukoClient.Models.Web.Responses
{
    public class ServerResponse : Response<BaseErrorJson>
    {
        public string IconUri { get; set; }
        public string Name { get; set; }
        public List<Channel> Channels { get; set; }
    }
}