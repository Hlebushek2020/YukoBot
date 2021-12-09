namespace YukoClient.Models.Web.Requests
{
    public class AuthorizationRequest : BaseRequest
    {
        public string Login { get; set; }
        public string Password { get; set; }
    }
}
