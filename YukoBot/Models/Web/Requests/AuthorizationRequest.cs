using YukoBot.Enums;
using YukoBot.Interfaces;

namespace YukoBot.Models.Web.Requests
{
    public class AuthorizationRequest : Request<AuthorizationRequest>, IBaseRequest
    {
        public string Token { get; set; }
        public RequestType Type { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }

    }
}
