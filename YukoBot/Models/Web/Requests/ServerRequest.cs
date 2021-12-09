using YukoBot.Enums;
using YukoBot.Interfaces;

namespace YukoBot.Models.Web.Requests
{
    public class ServerRequest : Request<ServerRequest>, IBaseRequest
    {
        public string Token { get; set; }
        public RequestType Type { get; set; }
        public ulong Id { get; set; }
    }
}
