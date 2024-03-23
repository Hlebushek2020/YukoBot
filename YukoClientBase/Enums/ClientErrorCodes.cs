namespace YukoClientBase.Enums
{
    public enum ClientErrorCodes
    {
        /// <summary>
        /// Essentially this code indicates that the error was not handled properly.
        /// </summary>
        UnhandledException = 0,

        /// <summary>
        /// The user token has expired but has not yet been deleted.
        /// </summary>
        TokenHasExpired = 1,

        /// <summary>
        /// The user is not authorized.
        /// </summary>
        NotAuthorized = 2,

        /// <summary>
        /// The user is not authenticated. Wrong login or password.
        /// </summary>
        InvalidCredentials = 3,

        /// <summary>
        /// This guild was not found.
        /// </summary>
        GuildNotFound = 4,

        /// <summary>
        /// This user is not a member of a particular server (guild) or does not have access to a specific channel.
        /// </summary>
        MemberNotFound = 5,

        /// <summary>
        /// This user is banned on a specific server (meaning a ban in a bot).
        /// </summary>
        MemberBanned = 6,

        /// <summary>
        /// This channel was not found.
        /// </summary>
        ChannelNotFound = 7,

        /// <summary>
        /// This message was not found.
        /// </summary>
        MessageNotFound = 8
    }
}