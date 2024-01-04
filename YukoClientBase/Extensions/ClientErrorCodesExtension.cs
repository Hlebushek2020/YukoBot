using YukoClientBase.Enums;
using YukoClientBase.Properties;

namespace YukoClientBase.Extensions
{
    public static class ClientErrorCodesExtension
    {
        public static string GetText(this ClientErrorCodes clientErrorCode) =>
            Resources.ResourceManager.GetString($"Enum.{nameof(ClientErrorCodes)}.{clientErrorCode}");

        public static string GetText(this ClientErrorCodes clientErrorCode, params object[] args)
        {
            string text = Resources.ResourceManager.GetString($"Enum.{nameof(ClientErrorCodes)}.{clientErrorCode}");
            return string.Format(text ?? string.Empty, args);
        }
    }
}