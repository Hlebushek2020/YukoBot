using System.Collections.ObjectModel;

namespace YukoClient.Models.Web.Responses
{
    public class ClientDataResponse : Response<ClientDataResponse>
    {
        public ulong Id { get; set; }
        public string AvatarUri { get; set; }
        public string Nikname { get; set; }
        public ObservableCollection<Server> Servers { get; set; }
    }
}