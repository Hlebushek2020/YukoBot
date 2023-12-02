namespace YukoBot.Models.Json;

public class UrlsErrorJson : BaseErrorJson
{
    public ulong? ChannelId { get; set; }
    public ulong? MessageId { get; set; }
}