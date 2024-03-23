using System;

namespace YukoClientBase.Interfaces
{
    public interface IUser
    {
        ulong UserId { get; set; }
        string AvatarUri { get; set; }
        string Username { get; set; }
    }
}