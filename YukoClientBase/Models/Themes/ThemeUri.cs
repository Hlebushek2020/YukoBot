using System;
using System.Collections.Generic;

namespace YukoClientBase.Models.Themes
{
    /// <summary>
    /// Contains constants for uniform theme resource identifiers and a method for obtaining such an identifier for a theme
    /// </summary>
    public sealed class ThemeUri
    {
        public static Uri Light { get; } =
            new Uri("/Sergey.UI.Extension;component/Resources/Themes/Light.xaml", UriKind.Relative);

        public static Uri Dark { get; } =
            new Uri("/Sergey.UI.Extension;component/Resources/Themes/Dark.xaml", UriKind.Relative);

        private static readonly Dictionary<Theme, Uri> _themes =
            new Dictionary<Theme, Uri> { { Theme.Light, Light }, { Theme.Dark, Dark } };

        /// <summary>
        /// Returns the <see cref="Uri"/> of the given theme
        /// </summary>
        /// <param name="theme"><see cref="Theme"/>Theme for which you need to get the Uri</param>
        /// <returns>Theme <see cref="Uri"/></returns>
        public static Uri Get(Theme theme)
        {
            return _themes[theme];
        }
    }
}