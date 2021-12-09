using Newtonsoft.Json;

namespace YukoBot.Models.Web.Responses
{
    public class Response
    {
        public string ErrorMessage { get; set; }
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
