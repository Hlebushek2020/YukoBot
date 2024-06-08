using System;

namespace YukoClientBase.Models.Themes.Attributes
{
    /// <summary>
    /// Specifies the value to display
    /// </summary>
    internal class DisplayAttribute : Attribute
    {
        public string Value { get; }

        public DisplayAttribute(string value)
        {
            Value = value;
        }
    }
}