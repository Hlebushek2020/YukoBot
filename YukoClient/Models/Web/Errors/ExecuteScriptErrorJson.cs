using YukoClientBase.Models.Web.Errors;

namespace YukoClient.Models.Web.Errors
{
    public class ExecuteScriptErrorJson : BaseErrorJson
    {
        public string Reason { get; set; }
    }
}