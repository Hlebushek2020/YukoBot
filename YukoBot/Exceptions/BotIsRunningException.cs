using System;

namespace YukoBot.Exceptions;

public class BotIsRunningException : Exception
{
    public BotIsRunningException() : base("The bot is already running") { }
}