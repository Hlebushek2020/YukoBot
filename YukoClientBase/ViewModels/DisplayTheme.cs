using System;
using System.Collections.Generic;
using System.Reflection;
using YukoClientBase.Models.Themes;
using YukoClientBase.Models.Themes.Attributes;

namespace YukoClientBase.ViewModels
{
    /// <summary>
    /// Used to display the theme title in Russian
    /// </summary>
    public struct DisplayTheme
    {
        /// <summary>
        /// Returns the theme
        /// </summary>
        public Theme Value { get; }

        /// <summary>
        /// Returns the title of the theme to display
        /// </summary>
        public string Display
        {
            get
            {
                Type type = Value.GetType();
                MemberInfo[] memInfo = type.GetMember(Value.ToString());
                if (memInfo.Length > 0)
                {
                    object[] attrs = memInfo[0].GetCustomAttributes(typeof(DisplayAttribute), false);
                    if (attrs.Length > 0)
                    {
                        return ((DisplayAttribute)attrs[0]).Value;
                    }
                }
                return Value.ToString();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayTheme"/> structure for the specified theme
        /// </summary>
        /// <param name="value">Theme</param>
        public DisplayTheme(Theme value)
        {
            Value = value;
        }

        /// <summary>
        /// Returns a list of themes to display
        /// </summary>
        /// <returns><see cref="DisplayTheme"/> list</returns>
        public static List<DisplayTheme> GetList()
        {
            List<DisplayTheme> displayThemes = new List<DisplayTheme>();
            foreach (Theme theme in Enum.GetValues(typeof(Theme)))
            {
                displayThemes.Add(new DisplayTheme(theme));
            }
            return displayThemes;
        }
    }
}