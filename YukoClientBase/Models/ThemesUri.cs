using System;
using System.Collections.Generic;

namespace YukoClientBase.Models
{
    /// <summary>
    /// Contains constants for uniform theme resource identifiers and a method for obtaining such an identifier
    /// for a theme
    /// </summary>
    public static class ThemesUri
    {
        private static readonly Uri _light = new Uri(
            "/YukoClientBase;component/Resources/Styles/Themes/Light.xaml",
            UriKind.Relative);

        private static readonly Uri _dark = new Uri(
            "/YukoClientBase;component/Resources/Styles/Themes/Dark.xaml",
            UriKind.Relative);

        private static readonly Dictionary<Themes, Uri> _themes = new Dictionary<Themes, Uri>
        {
            { Themes.Light, _light }, { Themes.Dark, _dark }
        };

        /// <summary>
        /// Returns the <see cref="Uri"/> of the given theme
        /// </summary>
        /// <param name="theme"><see cref="Themes"/>Theme for which you need to get the Uri</param>
        /// <returns>Theme <see cref="Uri"/></returns>
        public static Uri Get(Themes theme) => _themes[theme];
    }
}