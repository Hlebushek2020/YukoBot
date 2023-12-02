using System.Collections.Generic;
using YukoBot.Enums;

namespace YukoBot.Models.Json.Responses
{
    public class ServerBaseResponse : BaseResponse
    {
        public string IconUri { get; set; }
        public string Name { get; set; }
        public List<ChannelJson> Channels { get; set; } = new List<ChannelJson>();

        public static ServerBaseResponse FromServerJson(ServerJson serverJson)
        {
            if (serverJson == null)
                return new ServerBaseResponse { Error = new ErrorResponse { Code = ClientErrorCodes.MemberNotFound } };

            return new ServerBaseResponse
            {
                IconUri = serverJson.IconUri,
                Name = serverJson.Name,
                Channels = serverJson.Channels
            };
        }
    }
}