namespace YukoClientBase.Models.Web.Requests
{
    public class AuthorizationRequest : Request
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}