using System.Collections.Generic;

namespace YukoBot.Models.Web.Responses
{
    public class ServerBaseResponse : BaseResponse
    {
        public string IconUri { get; set; }
        public string Name { get; set; }
        public List<ChannelWeb> Channels { get; set; } = new List<ChannelWeb>();

        public static ServerBaseResponse FromServerWeb(ServerWeb serverWeb)
        {
            if (serverWeb == null)
            {
                return new ServerBaseResponse { ErrorMessage = "Вас нет на этом сервере!" };
            }
            return new ServerBaseResponse
            {
                IconUri = serverWeb.IconUri,
                Name = serverWeb.Name,
                Channels = serverWeb.Channels
            };
        }
    }
}