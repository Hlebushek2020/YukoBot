using System.Windows;
using YukoClient.Models;

namespace YukoClient
{
    public partial class ExecutionErrorsWindow : Window
    {
        public ExecutionErrorsWindow(Script script)
        {
            InitializeComponent();
            DataContext = script;
        }
    }
}