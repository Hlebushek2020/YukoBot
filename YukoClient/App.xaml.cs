using System;
using System.Reflection;
using System.Windows;
using YukoClient.Models;
using YukoClient.Models.Web;

namespace YukoClient
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const int BinaryFileVersion = 20211009;

        public static string Name { get; } = Assembly.GetExecutingAssembly().GetName().Name;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetTheme();
            MainWindow = new MainWindow();
            AuthorizationWindow authorization = new AuthorizationWindow();
            authorization.ShowDialog();
            if (!WebClient.TokenAvailability)
            {
                Shutdown();
            }
            else
            {
                MainWindow.Show();
            }
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