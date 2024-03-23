using System;

namespace YukoBot.Exceptions;

public class MessageRequestQueueServiceStoppedException : Exception
{
    public MessageRequestQueueServiceStoppedException() : base("Message request queue service has been stopped") { }
}