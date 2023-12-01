using Newtonsoft.Json;

namespace YukoBot.Models.Web.Responses
{
    public class BaseResponse
    {
        public ErrorResponse Error { get; set; }

        public override string ToString() => JsonConvert.SerializeObject(this);
    }
}