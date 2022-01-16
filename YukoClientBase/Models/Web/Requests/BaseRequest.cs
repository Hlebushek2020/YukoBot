using YukoClientBase.Enums;

namespace YukoClientBase.Models.Web.Requests
{
    public class BaseRequest : Request
    {
        public string Token { get; set; }
        public RequestType Type { get; set; }
    }
}