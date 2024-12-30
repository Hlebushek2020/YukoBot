namespace YukoClientBase.Enums
{
    public enum RequestType
    {
        Authorization = 1,
        RefreshToken = 2,
        GetServer = 4,
        GetServers = 8,
        ExecuteScripts = 16,
        GetMessageCollections = 32,
        GetUrls = 64
    }
}