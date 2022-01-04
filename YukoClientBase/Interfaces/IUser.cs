namespace YukoClientBase.Interfaces
{
    public interface IUser
    {
        ulong Id { get; set; }
        string AvatarUri { get; set; }
        string Nikname { get; set; }
    }
}
