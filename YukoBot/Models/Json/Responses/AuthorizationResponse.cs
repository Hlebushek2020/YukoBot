using System;

namespace YukoBot.Models.Json.Responses
{
    public class AuthorizationResponse : Response<BaseErrorJson>
    {
        public Guid Token { get; set; }
        public ulong UserId { get; set; }
        public string AvatarUri { get; set; }
        public string Username { get; set; }
    }
}