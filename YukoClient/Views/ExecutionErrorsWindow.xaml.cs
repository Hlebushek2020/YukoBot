using System.Windows;
using YukoClient.Models;
using YukoClient.ViewModels;

namespace YukoClient
{
    public partial class ExecutionErrorsWindow : Window
    {
        public ExecutionErrorsWindow(Script script)
        {
            InitializeComponent();
            DataContext = new ExecutionErrorsViewModel(Close, script);
        }
    }
}