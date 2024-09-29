using System;

namespace YukoClientBase.Exceptions
{
    public abstract class YukoException : Exception
    {
        protected YukoException(string message) : base(message) { }
    }
}