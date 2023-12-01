namespace YukoBot.Enums;

public enum ClientErrorCodes
{
    /// <summary>
    /// No errors occurred.
    /// </summary>
    None = 0,

    /// <summary>
    /// The user is not authorized or the token has expired.
    /// </summary>
    NotAuthorized = 1,

    /// <summary>
    /// The user is not authenticated. Wrong login or password.
    /// </summary>
    InvalidCredentials = 2,

    /// <summary>
    /// This user is not a member of a particular server (guild) or does not have access to a specific channel.
    /// </summary>
    MemberNotFound = 3,

    /// <summary>
    /// This user is banned on a specific server (meaning a ban in a bot).
    /// </summary>
    MemberBanned = 4,

    /// <summary>
    /// This channel was not found.
    /// </summary>
    ChannelNotFound = 5,

    /// <summary>
    /// This message was not found.
    /// </summary>
    MessageNotFound = 6
}