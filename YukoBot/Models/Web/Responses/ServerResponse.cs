using System.Collections.Generic;

namespace YukoBot.Models.Web.Responses
{
    public class ServerResponse : Response
    {
        public string IconUri { get; set; }
        public string Name { get; set; }
        public List<ChannelWeb> Channels { get; set; } = new List<ChannelWeb>();

        public static ServerResponse FromServerWeb(ServerWeb serverWeb)
        {
            if (serverWeb == null)
            {
                return new ServerResponse { ErrorMessage = "Вас нет на этом сервере!" };
            }
            return new ServerResponse
            {
                IconUri = serverWeb.IconUri,
                Name = serverWeb.Name,
                Channels = serverWeb.Channels
            };
        }
    }
}
