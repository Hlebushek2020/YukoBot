﻿using System.Collections.ObjectModel;
using YukoClientBase.Interfaces;
using YukoClientBase.Models.Web.Responses;

namespace YukoClient.Models.Web.Responses
{
    public class ClientDataResponse : Response, IClientData
    {
        public ulong Id { get; set; }
        public string AvatarUri { get; set; }
        public string Nikname { get; set; }
        public ObservableCollection<Server> Servers { get; set; }
    }
}