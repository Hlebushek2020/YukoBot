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
    private readonly ConcurrentDictionary<string, Metadata> _userTokens;
    private readonly IYukoSettings _yukoSettings;
    private readonly ILogger<TokenService> _logger;
    private readonly Timer _timer;

    public TokenService(IYukoSettings yukoSettings, ILogger<TokenService> logger)
    {
        if (yukoSettings.UserTokenRemovalTime < yukoSettings.UserTokenExpirationTime)
            throw new InvalidTokenLifetimeValueException();

        _yukoSettings = yukoSettings;
        _logger = logger;

        _userTokens = new ConcurrentDictionary<string, Metadata>();

        long dueTime = yukoSettings.UserTokenExpirationTime * 60000;
        _timer = new Timer(Action, null, dueTime, 120000);

        _logger.LogInformation($"{nameof(BotPingService)} loaded.");
    }

    ~TokenService() => _timer.Dispose();

    private void Action(object data)
    {
        DateTime currentDateTime = DateTime.UtcNow;
        foreach (KeyValuePair<string, Metadata> userToken in _userTokens)
        {
            DateTime forCompare = userToken.Value.StartUse.AddMinutes(_yukoSettings.UserTokenRemovalTime);
            if (forCompare >= currentDateTime)
            {
                _userTokens.TryRemove(userToken.Key, out _);
                _logger.LogInformation($"User token {userToken.Key} removed");
            }
        }
    }

    public bool GetUserId(string token, out ulong userId, out bool isExpired)
    {
        userId = ulong.MinValue;
        isExpired = false;

        if (!_userTokens.TryGetValue(token, out Metadata metadata))
            return false;

        userId = metadata.UserId;
        isExpired = metadata.StartUse.AddMinutes(_yukoSettings.UserTokenExpirationTime) >= DateTime.UtcNow;

        return true;
    }

    public string NewUserToken(ulong userId)
    {
        string userToken = Guid.NewGuid().ToString();
        Metadata metadata = new Metadata(userId);
        _userTokens.AddOrUpdate(userToken, x => metadata, (x, y) => metadata);
        return userToken;
    }

    public string RefreshUserToken(string userToken)
    {
        if (!_userTokens.TryGetValue(userToken, out Metadata oldMetadata))
            return userToken;

        string newUserToken = Guid.NewGuid().ToString();
        Metadata metadata = new Metadata(oldMetadata.UserId);
        _userTokens.TryUpdate(newUserToken, metadata, metadata);
        return newUserToken;
    }

    public string NewRefreshToken(out string refreshToken)
    {
        Random random = new Random();

        StringBuilder refreshTokenSb = new StringBuilder();
        while (refreshTokenSb.Length != 32)
            refreshTokenSb.Append((char)random.Next(33, 127));
        refreshToken = refreshTokenSb.ToString();

        return CreateHash(refreshToken);
    }

    public bool RefreshTokenCheck(string rtRequest, string rtDb) => rtDb.Equals(CreateHash(rtRequest));

    private static string CreateHash(string source)
    {
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(source));
        StringBuilder hashBuilder = new StringBuilder(hashBytes.Length / 2);
        foreach (byte code in hashBytes)
            hashBuilder.Append(code.ToString("X2"));
        return hashBuilder.ToString();
    }

    private struct Metadata
    {
        public ulong UserId { get; }
        public DateTime StartUse { get; }

        public Metadata(ulong userId)
        {
            UserId = userId;
            StartUse = DateTime.UtcNow;
        }
    }
}