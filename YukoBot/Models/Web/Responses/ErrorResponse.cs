using YukoBot.Enums;

namespace YukoBot.Models.Web.Responses;

public class ErrorResponse
{
    public ClientErrorCodes Code { get; set; }
}