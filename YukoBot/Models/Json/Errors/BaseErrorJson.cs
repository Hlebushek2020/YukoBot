using YukoBot.Enums;

namespace YukoBot.Models.Json.Errors;

public class BaseErrorJson
{
    public ClientErrorCodes Code { get; set; }
}