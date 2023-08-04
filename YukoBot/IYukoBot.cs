using System;

namespace YukoBot;

public interface IYukoBot
{
    DateTime StartDateTime { get; }
    void Shutdown(string reason = null);
}