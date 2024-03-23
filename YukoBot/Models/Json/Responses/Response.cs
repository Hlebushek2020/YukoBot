using Newtonsoft.Json;
using YukoBot.Models.Json.Errors;

namespace YukoBot.Models.Json.Responses
{
    public class Response<TError> where TError : BaseErrorJson
    {
        public TError Error { get; set; }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}