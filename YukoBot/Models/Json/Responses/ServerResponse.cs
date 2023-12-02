using System.Collections.Generic;
using YukoBot.Enums;

namespace YukoBot.Models.Json.Responses
{
    public class ServerResponse : Response<BaseErrorJson>
    {
        public string IconUri { get; set; }
        public string Name { get; set; }
        public List<ChannelJson> Channels { get; set; } = new List<ChannelJson>();

        public static ServerResponse FromServerJson(ServerJson serverJson)
        {
            if (serverJson == null)
                return new ServerResponse { Error = new BaseErrorJson { Code = ClientErrorCodes.MemberNotFound } };

            return new ServerResponse
            {
                IconUri = serverJson.IconUri,
                Name = serverJson.Name,
                Channels = serverJson.Channels
            };
        }
    }
}