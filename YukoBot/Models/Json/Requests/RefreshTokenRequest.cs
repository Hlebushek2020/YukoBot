namespace YukoBot.Models.Json.Requests;

public class RefreshTokenRequest : Request<RefreshTokenRequest>
{
    public string RefreshToken { get; set; }
}