namespace YukoClientBase.Interfaces
{
    public interface IClientData
    {
        ulong Id { get; set; }
        string AvatarUri { get; set; }
        string Nikname { get; set; }
    }
}
