using YukoBot.Models.Json.Errors;

namespace YukoBot.Models.Json.Responses
{
    public class AuthorizationResponse : Response<BaseErrorJson>
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
        public ulong UserId { get; set; }
        public string AvatarUri { get; set; }
        public string Username { get; set; }
        public bool TwoFactorAuthentication { get; set; }
    }
}