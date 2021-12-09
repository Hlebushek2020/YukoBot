using System;
using System.Windows;
using YukoClient.Models;
using YukoClient.ViewModels;

namespace YukoClient
{
    /// <summary>
    /// Логика взаимодействия для AddScriptWindow.xaml
    /// </summary>
    public partial class AddScriptWindow : Window
    {
        public AddScriptWindow(Server server)
        {
            InitializeComponent();
            DataContext = new AddScriptViewModel
            {
                Close = new Action(Close),
                Server = server
            };
        }
    }
}