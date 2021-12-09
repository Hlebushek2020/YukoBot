using System;

namespace YukoClient.Attributes.Enum
{
    public class DescriptionAttribute : Attribute
    {
        public string Value { get; }

        public DescriptionAttribute(string description) { Value = description; }
    }
}
