namespace YukoBot.Services;

public interface ITokenService
{
    string NewUserToken(ulong userId);
    string NewRefreshToken(ulong userId, out string refreshToken);
    bool UserTokenCheck(string userToken, out ulong userId, out bool isExpired);
    bool RefreshTokenCheck(string rtRequest, string rtDb);
    void GetPayloadFromToken(string token, out ulong userId, out bool isExpired);
}