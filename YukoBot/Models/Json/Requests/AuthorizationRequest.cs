namespace YukoBot.Models.Json.Requests
{
    public class AuthorizationRequest : BaseRequest<AuthorizationRequest>
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}