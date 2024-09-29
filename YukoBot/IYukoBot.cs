using System;

namespace YukoBot
{
    public interface IYukoBot
    {
        DateTime StartDateTime { get; }
        bool IsShutdown { get; }
        void Shutdown(string reason = null);
    }
}