using System.Collections.Generic;

namespace YukoBot.Models.Web.Responses
{
    public class ClientDataResponse : Response
    {
        public ulong Id { get; set; }
        public string AvatarUri { get; set; }
        public string Nikname { get; set; }
        public List<ServerResponse> Servers { get; set; } = new List<ServerResponse>();
    }
}
