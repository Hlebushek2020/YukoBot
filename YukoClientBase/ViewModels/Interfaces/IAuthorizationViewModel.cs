using System;
using System.Windows.Media;
using Prism.Commands;

namespace YukoClientBase.ViewModels.Interfaces
{
    public interface IAuthorizationViewModel
    {
        #region Propirties
        string Title { get; }
        string Login { get; set; }
        ImageBrush Logo { get; }
        bool IsRemember { get; set; }
        #endregion

        #region Commands
        DelegateCommand LoginCommand { get; }
        DelegateCommand SettingsCommand { get; }
        #endregion

        void SetCloseAction(Action closeAction);
        void SetGetPasswordFunc(Func<string> passwordFunc);
    }
}