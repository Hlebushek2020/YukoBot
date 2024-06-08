using YukoClientBase.Models.Themes.Attributes;

namespace YukoClientBase.Models.Themes
{
    /// <summary>
    /// Contains the constants of all available themes
    /// </summary>
    public enum Theme
    {
        [Display("Светлая")]
        Light = 0,

        [Display("Темная")]
        Dark = 1
    }
}