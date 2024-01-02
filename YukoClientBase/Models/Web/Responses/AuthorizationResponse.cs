using System;
using YukoClientBase.Interfaces;
using YukoClientBase.Models.Web.Errors;

namespace YukoClientBase.Models.Web.Responses
{
    public class AuthorizationResponse : Response<BaseErrorJson>, IUser
    {
        public Guid Token { get; set; }
        public ulong UserId { get; set; }
        public string AvatarUri { get; set; }
        public string Username { get; set; }
    }
}