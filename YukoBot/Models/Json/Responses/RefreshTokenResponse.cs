using YukoBot.Models.Json.Errors;

namespace YukoBot.Models.Json.Responses;

public class RefreshTokenResponse : Response<BaseErrorJson>
{
    public string Token { get; set; }
    public string RefreshToken { get; set; }
}