using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using YukoCollectionsClient.Models;

namespace YukoCollectionsClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string Name { get; } = Assembly.GetExecutingAssembly().GetName().Name;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetTheme();
            MainWindow = new MainWindow();
            MainWindow.Show();
        }

        public static void SetTheme()
        {
            Uri uri = new Uri($"Resources/Themes/{Settings.Current.Theme}.xaml", UriKind.Relative);
            ResourceDictionary resource = (ResourceDictionary)LoadComponent(uri);
            Current.Resources.MergedDictionaries.Clear();
            Current.Resources.MergedDictionaries.Add(resource);
        }
    }
}