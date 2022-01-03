using System.Collections.Generic;
using YukoClientBase.Interfaces;
using YukoClientBase.Models.Web.Responses;

namespace YukoCollectionsClient.Models.Web.Responses
{
    public class ClientDataResponse : Response, IClientData
    {
        public ulong Id { get; set; }
        public string AvatarUri { get; set; }
        public string Nikname { get; set; }
        public List<MessageCollection> MessageCollections { get; set; }
    }
}
