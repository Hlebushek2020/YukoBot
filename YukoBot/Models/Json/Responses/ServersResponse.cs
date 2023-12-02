using System.Collections.Generic;

namespace YukoBot.Models.Json.Responses
{
    public class ServersResponse : Response<BaseErrorJson>
    {
        public List<ServerJson> Servers { get; set; } = new List<ServerJson>();
    }
}