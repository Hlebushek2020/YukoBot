using System;

namespace YukoBot.Exceptions;

public class InvalidTokenLifetimeValueException : Exception
{
    public InvalidTokenLifetimeValueException() : base(
        """
        The token validity period is longer than the deletion time. The value of parameter UserTokenRemovalTime
        cannot be less than the value of parameter UserTokenExpirationTime.
        """)
    {
    }
}