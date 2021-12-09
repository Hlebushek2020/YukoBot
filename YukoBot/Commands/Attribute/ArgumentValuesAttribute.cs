using System.Collections.Generic;

namespace YukoBot.Commands.Attribute
{
    public sealed class ArgumentValuesAttribute : System.Attribute
    {
        public IReadOnlyList<string> ArgumentValues { get; }
        public ArgumentValuesAttribute(params string[] values)
        {
            ArgumentValues = new List<string>(values);
        }
    }
}
