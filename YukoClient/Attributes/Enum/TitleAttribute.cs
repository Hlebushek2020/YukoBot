using System;

namespace YukoClient.Attributes.Enum
{
    public class TitleAttribute : Attribute
    {
        public string Value { get; }

        public TitleAttribute(string title) { Value = title; }
    }
}
