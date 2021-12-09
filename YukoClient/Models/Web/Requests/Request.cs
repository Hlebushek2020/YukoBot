using Newtonsoft.Json;

namespace YukoClient.Models.Web.Requests
{
    public class Request
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}