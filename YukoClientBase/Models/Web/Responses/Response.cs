using YukoClientBase.Models.Web.Errors;

namespace YukoClientBase.Models.Web.Responses
{
    public class Response<TError> where TError : BaseErrorJson
    {
        public TError Error { get; set; }
    }
}