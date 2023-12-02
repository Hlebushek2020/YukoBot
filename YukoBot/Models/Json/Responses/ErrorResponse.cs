using YukoBot.Enums;

namespace YukoBot.Models.Json.Responses;

public class ErrorResponse : IErrorBaseJson
{
    public ClientErrorCodes Code { get; set; }
}