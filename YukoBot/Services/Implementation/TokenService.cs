using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using YukoBot.Exceptions;

namespace YukoBot.Services.Implementation;

public class TokenService : ITokenService
{
    private readonly ConcurrentDictionary<string, DateTime> _userTokens;
    private readonly IYukoSettings _yukoSettings;
    private readonly ILogger<TokenService> _logger;
    private readonly Timer _timer;

    public TokenService(IYukoSettings yukoSettings, ILogger<TokenService> logger)
    {
        if (yukoSettings.RefreshTokenLifeInHours * 60 < yukoSettings.TokenLifeInMinutes)
            throw new InvalidTokenLifetimeValueException();

        _yukoSettings = yukoSettings;
        _logger = logger;

        _userTokens = new ConcurrentDictionary<string, DateTime>();

        long dueTime = yukoSettings.TokenLifeInMinutes * 60000;
        _timer = new Timer(Action, null, dueTime, 60000);

        _logger.LogInformation($"{nameof(TokenService)} loaded.");
    }

    ~TokenService() => _timer.Dispose();

    private void Action(object data)
    {
        _logger.LogInformation("Clearing expired authorization tokens");
        DateTime currentDateTime = DateTime.UtcNow;
        foreach (KeyValuePair<string, DateTime> userToken in _userTokens)
        {
            DateTime expiredDatetime = userToken.Value
                .AddMinutes(_yukoSettings.TokenLifeInMinutes);

            if (expiredDatetime > currentDateTime)
                continue;

            _userTokens.TryRemove(userToken.Key, out _);
        }
    }

    public string NewUserToken(ulong userId)
    {
        DateTime expired = DateTime.UtcNow.AddMinutes(_yukoSettings.TokenLifeInMinutes);
        string payload = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}|{expired.Ticks}"));
        string token = GenerateRandom().Insert(0, $"{payload}.").ToString();
        _userTokens.AddOrUpdate(CreateHash(token), x => expired, (x, y) => expired);
        return token;
    }

    public string NewRefreshToken(ulong userId, out string refreshToken)
    {
        DateTime expired = DateTime.UtcNow.AddHours(_yukoSettings.RefreshTokenLifeInHours);
        string payload = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{userId}|{expired.Ticks}"));
        refreshToken = GenerateRandom().Insert(0, $"{payload}.").ToString();
        return CreateHash(refreshToken);
    }

    public bool UserTokenCheck(string userToken, out ulong userId, out bool isExpired)
    {
        userId = default;
        isExpired = default;

        if (!_userTokens.ContainsKey(CreateHash(userToken)))
            return false;

        GetPayloadFromToken(userToken, out userId, out isExpired);

        return true;
    }

    public bool RefreshTokenCheck(string rtRequest, string rtDb) => rtDb.Equals(CreateHash(rtRequest));

    public void GetPayloadFromToken(string token, out ulong userId, out bool isExpired)
    {
        string payloadBase64 = token[..token.IndexOf('.')];
        string payload = Encoding.UTF8.GetString(Convert.FromBase64String(payloadBase64));
        string[] payloadParts = payload.Split('|');

        userId = Convert.ToUInt64(payloadParts[0]);

        DateTime expiredUtc = new DateTime(Convert.ToInt64(payloadParts[1]));
        isExpired = DateTime.UtcNow > expiredUtc;
    }

    private static StringBuilder GenerateRandom()
    {
        Random random = new Random();

        StringBuilder refreshTokenSb = new StringBuilder();
        while (refreshTokenSb.Length != 32)
            refreshTokenSb.Append((char)random.Next(33, 127));

        return refreshTokenSb;
    }

    private static string CreateHash(string source)
    {
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        StringBuilder hashBuilder = new StringBuilder(hashBytes.Length / 2);
        foreach (byte code in hashBytes)
            hashBuilder.Append(code.ToString("X2"));
        return hashBuilder.ToString();
    }
}