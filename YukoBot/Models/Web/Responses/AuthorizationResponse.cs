namespace YukoBot.Models.Web.Responses
{
    public class AuthorizationResponse : Response
    {
        public string Token { get; set; }
        public ulong Id { get; set; }
        public string AvatarUri { get; set; }
        public string Nikname { get; set; }
    }
}
