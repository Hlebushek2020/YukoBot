namespace YukoBot.Services;

public interface ITokenService
{
    bool GetUserId(string token, out ulong userId, out bool isExpired);
    string NewUserToken(ulong userId);
    string RefreshUserToken(string userToken);
    string NewRefreshToken(out string refreshToken);
    bool RefreshTokenCheck(string rtRequest, string rtDb);
}