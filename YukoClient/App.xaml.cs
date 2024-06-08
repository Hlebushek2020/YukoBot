using System;
using System.Reflection;
using System.Threading;
using System.Windows;
using YukoClient.Models.Web;
using YukoClient.ViewModels;
using YukoClientBase.Models;
using YukoClientBase.Models.Themes;
using YukoClientBase.Views;
using MessageBox = YukoClientBase.Dialogs.MessageBox;

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
            SwitchTheme(null);
            _yukoClientMutex = new Mutex(true, Settings.YukoClientMutexName, out bool createdNew);
            if (!createdNew)
            {
                MessageBox.Show("Клиент уже открыт! Запрещено открывать несколько клиентов.", Name,
                    MessageBoxButton.OK, MessageBoxImage.Warning);
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

        public static void SwitchTheme(Theme? theme)
        {
            Settings settings = Settings.Current;
            if (!theme.HasValue || settings.Theme != theme.Value)
            {
                Uri uri = ThemeUri.Get(settings.Theme);
                if (theme.HasValue)
                {
                    settings.Theme = theme.Value;
                    uri = ThemeUri.Get(theme.Value);
                }
                ResourceDictionary resource = (ResourceDictionary)LoadComponent(uri);
                Current.Resources.MergedDictionaries.Clear();
                Current.Resources.MergedDictionaries.Add(resource);
            }
        }
    }
}