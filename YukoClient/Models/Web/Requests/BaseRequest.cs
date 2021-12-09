using YukoClient.Enums;
using YukoClient.Interfaces.Model;

namespace YukoClient.Models.Web.Requests
{
    public class BaseRequest : Request, IToken, IRequestType
    {
        public string Token { get; set; }
        public RequestType Type { get; set; }
    }
}