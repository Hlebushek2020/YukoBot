using System.Windows;
using MessageBox = Sergey.UI.Extension.Dialogs.MessageBox;

namespace YukoClient
{
    public partial class InputWindow : Window
    {
        public InputWindow() { InitializeComponent(); }

        public string GetInputValue() => tb_input.Text;

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tb_input.Text))
            {
                MessageBox.Show("Пустое значение недопустимо так же как и значение состоящее из пробелов.", "",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Close();
            }
        }
    }
}