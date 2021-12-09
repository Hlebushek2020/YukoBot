using YukoBot.Enums;
using YukoBot.Interfaces;

namespace YukoBot.Models.Web.Requests
{
    public class BaseRequest : Request<BaseRequest>, IBaseRequest
    {
        public string Token { get; set; }
        public RequestType Type { get; set; }
    }
}
