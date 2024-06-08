using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace YukoClientBase.Views
{
    /// <summary>
    /// Логика взаимодействия для MessageBoxWindow.xaml
    /// </summary>
    internal partial class MessageBoxWindow : Window
    {
        public MessageBoxResult Result { get; private set; }

        public MessageBoxWindow(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            InitializeComponent();
            MaxHeight = (int)(SystemParameters.PrimaryScreenHeight / 2);
            MaxWidth = (int)(SystemParameters.PrimaryScreenWidth / 2);
            Title = caption;
            textBlock_Message.Text = messageBoxText;
            switch (button)
            {
                case MessageBoxButton.OK:
                    button_OK.Visibility = Visibility.Visible;
                    Result = MessageBoxResult.OK;
                    break;
                case MessageBoxButton.OKCancel:
                    button_OK.Visibility = Visibility.Visible;
                    button_Cansel.Visibility = Visibility.Visible;
                    Result = MessageBoxResult.Cancel;
                    break;
                case MessageBoxButton.YesNo:
                    button_Yes.Visibility = Visibility.Visible;
                    button_No.Visibility = Visibility.Visible;
                    Result = MessageBoxResult.No;
                    break;
                case MessageBoxButton.YesNoCancel:
                    button_Yes.Visibility = Visibility.Visible;
                    button_No.Visibility = Visibility.Visible;
                    button_Cansel.Visibility = Visibility.Visible;
                    Result = MessageBoxResult.Cancel;
                    break;
            }
            switch (icon)
            {
                case MessageBoxImage.Error:
                    image_Icon.Source =
                        new BitmapImage(new Uri("/Sergey.UI.Extension;component/Resources/Images/dialog-error-64.png",
                            UriKind.Relative));
                    break;
                case MessageBoxImage.Information:
                    image_Icon.Source =
                        new BitmapImage(new Uri(
                            "/Sergey.UI.Extension;component/Resources/Images/dialog-information-64.png",
                            UriKind.Relative));
                    break;
                case MessageBoxImage.Question:
                    image_Icon.Source =
                        new BitmapImage(new Uri(
                            "/Sergey.UI.Extension;component/Resources/Images/dialog-question-64.png",
                            UriKind.Relative));
                    break;
                case MessageBoxImage.Warning:
                    image_Icon.Source =
                        new BitmapImage(new Uri("/Sergey.UI.Extension;component/Resources/Images/dialog-warning-64.png",
                            UriKind.Relative));
                    break;
            }
        }

        private void Button_MBR_Click(object sender, RoutedEventArgs e)
        {
            string button = ((Button)sender).Tag.ToString();
            Result = (MessageBoxResult)Convert.ToInt32(button);
            Close();
        }
    }
}