using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using YukoClientBase.Models;
using YukoCollectionsClient.Models.Web;

namespace YukoCollectionsClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string Name { get; } = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;

        private static Mutex yukoClientMutex;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SetTheme();
            yukoClientMutex = new Mutex(true, Settings.YukoClientMutexName, out bool createdNew);
            if (!createdNew)
            {
                Models.Dialogs.MessageBox.Show("Клиент уже открыт! Запрещено открывать несколько клиентов.", Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown();
            }
            MainWindow = new MainWindow();
            AuthorizationWindow authorization = new AuthorizationWindow();
            authorization.ShowDialog();
            if (!WebClient.Current.TokenAvailability)
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