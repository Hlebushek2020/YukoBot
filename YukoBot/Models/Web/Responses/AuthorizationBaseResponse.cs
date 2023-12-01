using System;

namespace YukoBot.Models.Web.Responses
{
    public class AuthorizationBaseResponse : BaseResponse
    {
        public Guid Token { get; set; }
        public ulong Id { get; set; }
        public string AvatarUri { get; set; }
        public string Username { get; set; }
    }
}