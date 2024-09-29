using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using YukoClient.Models.Web;
using YukoClient.ViewModels;
using YukoClientBase.Models;
using YukoClientBase.Views;
using MessageBox = YukoClientBase.Dialogs.MessageBox;
using BaseResources = YukoClientBase.Properties.Resources;

namespace YukoClient
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public const int BinaryFileVersion = 20211009;

        public static string Name { get; } =
            Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyTitleAttribute>().Title;

        private static Mutex _yukoClientMutex;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // set the theme
            Uri themeUri = ThemesUri.Get(Settings.Current.Theme);
            ResourceDictionary resource = (ResourceDictionary)LoadComponent(themeUri);
            Resources.MergedDictionaries.Add(resource);

            // check whether one of the clients is running or not
            _yukoClientMutex = new Mutex(true, Settings.YukoClientMutexName, out bool createdNew);
            if (!createdNew)
            {
                MessageBox.Show(BaseResources.App_AlreadyLaunched, Name, MessageBoxButton.OK, MessageBoxImage.Warning);
                Shutdown();
            }

            MainWindow = new MainWindow();

            AuthorizationWindow authorization = new AuthorizationWindow(new AuthorizationViewModel());
            authorization.ShowDialog();

            if (!WebClient.Current.TokenAvailability)
                Shutdown();
            else
            {
                MainWindow.WindowStyle = authorization.WindowStyle;
                MainWindow.WindowState = authorization.WindowState;
                MainWindow.Show();
            }
        }
    }
}