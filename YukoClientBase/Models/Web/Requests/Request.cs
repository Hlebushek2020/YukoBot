using Newtonsoft.Json;

namespace YukoClientBase.Models.Web.Requests
{
    public class Request
    {
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}