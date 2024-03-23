using System;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YukoClientBase.ViewModels;
using YukoCollectionsClient.Models;
using YukoCollectionsClient.Models.Web;

namespace YukoCollectionsClient.ViewModels
{
    public class AuthorizationViewModel : BaseAuthorizationViewModel
    {
        #region Propirties
        public override string Title => App.Name;

        public override ImageBrush Logo
        {
            get
            {
                BitmapDecoder decoder = BitmapDecoder.Create(
                    new Uri("pack://application:,,,/../program-icon.ico"),
                    BitmapCreateOptions.DelayCreation,
                    BitmapCacheOption.OnDemand);

                BitmapFrame bitmapFrame = decoder.Frames.Where(f => f.Width <= 256)
                        .OrderByDescending(f => f.Width).FirstOrDefault() ??
                    decoder.Frames.OrderBy(f => f.Width).FirstOrDefault();

                return new ImageBrush { Stretch = Stretch.Uniform, ImageSource = bitmapFrame };
            }
        }
        #endregion

        public AuthorizationViewModel() : base(Storage.Current, WebClient.Current) { }
    }
}