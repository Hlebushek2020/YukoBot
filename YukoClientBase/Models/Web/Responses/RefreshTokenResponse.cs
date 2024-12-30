using YukoClientBase.Models.Web.Errors;

namespace YukoClientBase.Models.Web.Responses
{
    public class RefreshTokenResponse : Response<BaseErrorJson>
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}