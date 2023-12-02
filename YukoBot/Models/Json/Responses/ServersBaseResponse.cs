using System.Collections.Generic;

namespace YukoBot.Models.Json.Responses
{
    public class ServersBaseResponse : BaseResponse
    {
        public List<ServerJson> Servers { get; set; } = new List<ServerJson>();
    }
}