using YukoBot.Enums;

namespace YukoBot.Models.Json.Responses;

public interface IErrorBaseJson
{
    public ClientErrorCodes Code { get; set; }
}