namespace YukoBot.Models.Json.Requests
{
    public class AuthorizationRequest : Request<AuthorizationRequest>
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}