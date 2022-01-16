
using YukoClientBase.Interfaces;

namespace YukoClientBase.Models.Web.Responses
{
    public class AuthorizationResponse : Response, IUser
    {
        public string Token { get; set; }
        public ulong Id { get; set; }
        public string AvatarUri { get; set; }
        public string Nikname { get; set; }
    }
}
