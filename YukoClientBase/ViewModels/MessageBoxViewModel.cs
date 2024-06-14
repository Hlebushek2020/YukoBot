using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YukoClientBase.ViewModels
{
    internal class MessageBoxViewModel : BindableBase
    {
        #region Fields (Static Readonly)
        private static readonly Uri _errorIcon = new Uri(
            "/YukoClientBase;component/Resources/dialog-error-64.png",
            UriKind.Relative);

        private static readonly Uri _warningIcon = new Uri(
            "/YukoClientBase;component/Resources/dialog-warning-64.png",
            UriKind.Relative);

        private static readonly Uri _informationIcon = new Uri(
            "/YukoClientBase;component/Resources/dialog-information-64.png",
            UriKind.Relative);

        private static readonly Uri _questionIcon = new Uri(
            "/YukoClientBase;component/Resources/dialog-question-64.png",
            UriKind.Relative);
        #endregion

        #region Properties
        public double MaxHeight { get; }
        public string Caption { get; }
        public ImageSource Image { get; }
        public bool ShowImage { get; }
        public string Text { get; }
        public bool OkButtonEnabled { get; }
        public bool YesButtonEnabled { get; }
        public bool NoButtonEnabled { get; }
        public bool CancelButtonEnabled { get; }
        public MessageBoxResult Result { get; private set; }
        #endregion

        public DelegateCommand<object> ButtonCommand { get; }

        #region CloseCallbackEvent
        public delegate void CloseCallbackEventHandler();
        public event CloseCallbackEventHandler CloseCallback;
        protected virtual void OnCloseCallback() => CloseCallback?.Invoke();
        #endregion

        public MessageBoxViewModel(
            string messageBoxText,
            string caption,
            MessageBoxButton button,
            MessageBoxImage icon)
        {
            MaxHeight = (int)(SystemParameters.PrimaryScreenHeight / 2);

            Caption = caption;
            Text = messageBoxText;

            switch (button)
            {
                case MessageBoxButton.OKCancel:
                    OkButtonEnabled = true;
                    CancelButtonEnabled = true;
                    Result = MessageBoxResult.Cancel;
                    break;

                case MessageBoxButton.YesNo:
                    YesButtonEnabled = true;
                    NoButtonEnabled = true;
                    Result = MessageBoxResult.No;
                    break;

                case MessageBoxButton.YesNoCancel:
                    YesButtonEnabled = true;
                    NoButtonEnabled = true;
                    CancelButtonEnabled = true;
                    Result = MessageBoxResult.Cancel;
                    break;

                case MessageBoxButton.OK:
                default:
                    OkButtonEnabled = true;
                    Result = MessageBoxResult.OK;
                    break;
            }

            ShowImage = true;
            switch (icon)
            {
                case MessageBoxImage.Error:
                    Image = new BitmapImage(_errorIcon);
                    break;

                case MessageBoxImage.Warning:
                    Image = new BitmapImage(_warningIcon);
                    break;

                case MessageBoxImage.Information:
                    Image = new BitmapImage(_informationIcon);
                    break;

                case MessageBoxImage.Question:
                    Image = new BitmapImage(_questionIcon);
                    break;

                case MessageBoxImage.None:
                default:
                    ShowImage = false;
                    break;
            }

            ButtonCommand = new DelegateCommand<object>(
                result =>
                {
                    Result = (MessageBoxResult)result;
                    OnCloseCallback();
                });
        }
    }
}