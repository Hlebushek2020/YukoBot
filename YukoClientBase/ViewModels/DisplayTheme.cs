using System;
using System.Collections.Generic;
using YukoClientBase.Models;
using YukoClientBase.Properties;

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
        public Themes Value { get; }

        /// <summary>
        /// Returns the title of the theme to display
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayTheme"/> structure for the specified theme
        /// </summary>
        /// <param name="value">Theme</param>
        public DisplayTheme(Themes value)
        {
            Value = value;

            string resourceKey = $"Enum.{nameof(Themes)}.{value.ToString()}";
            Title = Resources.ResourceManager.GetString(resourceKey) ?? value.ToString();
        }

        /// <summary>
        /// Returns a list of themes to display
        /// </summary>
        /// <returns><see cref="DisplayTheme"/> list</returns>
        public static List<DisplayTheme> GetList()
        {
            List<DisplayTheme> displayThemes = new List<DisplayTheme>();
            foreach (Themes theme in Enum.GetValues(typeof(Themes)))
                displayThemes.Add(new DisplayTheme(theme));
            return displayThemes;
        }
    }
}