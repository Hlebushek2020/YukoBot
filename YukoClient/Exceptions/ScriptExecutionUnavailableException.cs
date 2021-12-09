using System;

namespace YukoClient.Exceptions
{
    public class ScriptExecutionUnavailableException : Exception
    {
        public override string Message => "Script execution unavailable";
    }
}
