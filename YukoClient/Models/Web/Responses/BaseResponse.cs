using Newtonsoft.Json;

namespace YukoClient.Models.Web.Responses
{
    public class BaseResponse<T>
    {
        public string ErrorMessage { get; set; }

        public static T FromJson(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
