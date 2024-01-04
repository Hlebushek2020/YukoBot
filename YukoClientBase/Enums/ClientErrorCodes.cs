namespace YukoClientBase.Enums
{
    public enum ClientErrorCodes
    {
        /// <summary>
        /// Essentially this code indicates that the error was not handled properly
        /// </summary>
        UnhandledException = 0,

        /// <summary>
        /// The user is not authorized or the token has expired.
        /// </summary>
        NotAuthorized = 1,

        /// <summary>
        /// The user is not authenticated. Wrong login or password.
        /// </summary>
        InvalidCredentials = 2,

        /// <summary>
        /// This guild was not found.
        /// </summary>
        GuildNotFound = 3,

        /// <summary>
        /// This user is not a member of a particular server (guild) or does not have access to a specific channel.
        /// </summary>
        MemberNotFound = 4,

        /// <summary>
        /// This user is banned on a specific server (meaning a ban in a bot).
        /// </summary>
        MemberBanned = 5,

        /// <summary>
        /// This channel was not found.
        /// </summary>
        ChannelNotFound = 6,

        /// <summary>
        /// This message was not found.
        /// </summary>
        MessageNotFound = 7
    }
}