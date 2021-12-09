using YukoBot.Enums;

namespace YukoBot.Interfaces
{
    public interface IBaseRequest
    {
        string Token { get; set; }
        RequestType Type { get; set; }
    }
}
