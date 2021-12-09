using Newtonsoft.Json;

namespace YukoBot.Models.Web.Requests
{
    public class Request<T>
    {
        public static T FromJson(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
