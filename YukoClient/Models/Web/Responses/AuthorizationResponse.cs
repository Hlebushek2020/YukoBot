using YukoClient.Interfaces.Model;

namespace YukoClient.Models.Web.Responses
{
    public class AuthorizationResponse : Response<AuthorizationResponse>, IToken
    {
        public string Token { get; set; }
    }
}
