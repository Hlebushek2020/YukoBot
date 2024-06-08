using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using MessageBox = YukoClientBase.Dialogs.MessageBox;

namespace YukoClientBase.Views
{
    public partial class CodeInputWindow : Window
    {
        private static readonly Regex _digitTexRegex = new Regex("^[0-9]+", RegexOptions.Compiled);

        private bool _okButtonIsPressed;

        public CodeInputWindow(string title)
        {
            InitializeComponent();
            Title = title;

            Closing += OnClosing;
        }

        public string GetInputValue() => Input.Text;

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Input.Text))
            {
                MessageBox.Show("Пустое значение недопустимо так же как и значение состоящее из пробелов.", "",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                _okButtonIsPressed = true;
                Close();
            }
        }

        private void Input_OnPreviewTextInput(object sender, TextCompositionEventArgs e) =>
            e.Handled = !_digitTexRegex.IsMatch(e.Text.Trim());

        private void OnClosing(object sender, CancelEventArgs e) => e.Cancel = !_okButtonIsPressed;
    }
}