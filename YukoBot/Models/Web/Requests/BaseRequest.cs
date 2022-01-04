using YukoBot.Enums;

namespace YukoBot.Models.Web.Requests
{
    public class BaseRequest : BaseRequest<BaseRequest> { }

    public class BaseRequest<T> : Request<T>
    {
        public string Token { get; set; }
        public RequestType Type { get; set; }
    }
}