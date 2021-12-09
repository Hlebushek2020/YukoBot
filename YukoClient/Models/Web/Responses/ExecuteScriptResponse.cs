using System.Collections.Generic;

namespace YukoClient.Models.Web.Responses
{
    public class ExecuteScriptResponse : Response<ExecuteScriptResponse>
    {
        public bool Next { get; set; }
        public List<string> Urls { get; set; } = new List<string>();
    }
}
