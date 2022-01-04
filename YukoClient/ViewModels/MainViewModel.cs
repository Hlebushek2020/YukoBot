using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using YukoClient.Models;
using YukoClient.Models.Progress;
using YukoClientBase.Interfaces;

namespace YukoClient.ViewModels
{
    public class MainViewModel : BindableBase, ICloseableView, IViewTitle
    {
        #region Fields
        private Server selectedServer;
        #endregion

        #region Propirties
        public string Title { get => App.Name; }
        public Action Close { get; set; }
        public ImageBrush Avatar { get { return Storage.Current.Avatar; } }
        public string Nikname { get { return Storage.Current.Nikname; } }
        public string Id { get { return Storage.Current.Id.ToString(); } }
        public ObservableCollection<Server> Servers { get { return Storage.Current.Servers; } }
        public Server SelectedServer
        {
            get { return selectedServer; }
            set
            {
                selectedServer = value;
                if (selectedServer != null)
                {
                    RaisePropertyChanged("Scripts");
                    RaisePropertyChanged("Urls");
                }
            }
        }
        public ObservableCollection<Script> Scripts { get { return selectedServer?.Scripts; } }
        public Script SelectedScript { get; set; }
        public ObservableCollection<string> Urls { get { return selectedServer?.Urls; } }
        public string SelectedUrl { get; set; }
        #endregion

        #region Commands
        public DelegateCommand WindowLoadedCommand { get; }
        // User Commands
        public DelegateCommand AppSettingsCommand { get; }
        // Server Commands
        public DelegateCommand ServerSettingsCommand { get; }
        public DelegateCommand RemoveServerCommand { get; }
        public DelegateCommand UpdateServerCollectionCommand { get; }
        // Script Commands
        public DelegateCommand AddScriptCommand { get; }
        public DelegateCommand RemoveScriptCommand { get; }
        public DelegateCommand ClearScriptsCommand { get; }
        public DelegateCommand ExportScriptsCommand { get; }
        public DelegateCommand ImportScriptsCommand { get; }
        public DelegateCommand RunScriptsCommand { get; }
        // Url Command
        public DelegateCommand RemoveUrlCommand { get; }
        public DelegateCommand ClearUrlsCommand { get; }
        public DelegateCommand ExportUrlsCommand { get; }
        public DelegateCommand ImportUrlsCommand { get; }
        public DelegateCommand DownloadFilesCommand { get; }
        #endregion

        public MainViewModel()
        {
            Storage.Current.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };
            WindowLoadedCommand = new DelegateCommand(() =>
            {
                // TODO: Update servers (oerride)
                ProgressWindow progress = new ProgressWindow(new StorageInitialization());
                progress.ShowDialog();
            });
            // User Commands
            AppSettingsCommand = new DelegateCommand(() =>
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.ShowDialog();
            });
            // Server Commands
            UpdateServerCollectionCommand = new DelegateCommand(() =>
            {
                MessageBoxResult messageResult = Models.Dialogs.MessageBox.Show("Перезаписать данные текущих серверов? Внимание! Это приведет к потере списка правил и ссылок.", App.Name, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (messageResult != MessageBoxResult.Cancel)
                {
                    ProgressWindow progress = new ProgressWindow(new UpdateServers(messageResult == MessageBoxResult.Yes));
                    progress.ShowDialog();
                }
            });
            RemoveServerCommand = new DelegateCommand(() =>
            {
                if (selectedServer != null)
                {
                    if (Models.Dialogs.MessageBox.Show($"Удалить сервер {selectedServer.Name} из списка?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        Storage.Current.Servers.Remove(selectedServer);
                    }
                }
            });
            ServerSettingsCommand = new DelegateCommand(() =>
            {
                if (selectedServer != null)
                {
                    ServerSettingsWindow serverSettings = new ServerSettingsWindow(selectedServer);
                    serverSettings.ShowDialog();
                    // save data
                    Task.Run(() => Storage.Current.Save());
                }
            });
            // Script Commands
            AddScriptCommand = new DelegateCommand(() =>
            {
                if (selectedServer != null)
                {
                    AddScriptWindow addScript = new AddScriptWindow(selectedServer);
                    addScript.ShowDialog();
                }
                else
                {
                    Models.Dialogs.MessageBox.Show("Выберите сервер!", App.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });
            RemoveScriptCommand = new DelegateCommand(() =>
            {
                if (SelectedScript != null)
                {
                    if (Models.Dialogs.MessageBox.Show("Удалить выбранное правило?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        selectedServer.Scripts.Remove(SelectedScript);
                    }
                }
                else
                {
                    Models.Dialogs.MessageBox.Show("Выберите сервер!", App.Name, MessageBoxButton.OK, MessageBoxImage.Information);
                }
            });
            ClearScriptsCommand = new DelegateCommand(() =>
            {
                if (selectedServer != null && selectedServer.Scripts.Count > 0)
                {
                    if (Models.Dialogs.MessageBox.Show("Очистить список правил?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        selectedServer.Scripts.Clear();
                    }
                }
            });
            ExportScriptsCommand = new DelegateCommand(() =>
            {
                if (selectedServer != null && selectedServer.Scripts.Count > 0)
                {
                    using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog
                    {
                        Filter = "Yuko Script|*.yukoscript",
                        DefaultExt = "yukoscript"
                    })
                    {
                        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            ProgressWindow progressWindow = new ProgressWindow(new ExportScripts(selectedServer.Scripts, selectedServer.Id, saveFileDialog.FileName));
                            progressWindow.ShowDialog();
                        }
                    }
                }
            });
            ImportScriptsCommand = new DelegateCommand(() =>
            {
                if (selectedServer != null)
                {
                    using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
                    {
                        Filter = "Yuko Script|*.yukoscript",
                        DefaultExt = "yukoscript"
                    })
                    {
                        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            if (selectedServer.Scripts.Count > 0)
                            {
                                if (Models.Dialogs.MessageBox.Show("Очистить список правил перед добавлением?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    selectedServer.Scripts.Clear();
                                }
                            }
                            ProgressWindow progressWindow = new ProgressWindow(new ImportScripts(selectedServer.Scripts, selectedServer.Id, openFileDialog.FileName));
                            progressWindow.ShowDialog();
                        }
                    }
                }
            });
            RunScriptsCommand = new DelegateCommand(() =>
            {
                if (selectedServer != null && selectedServer.Scripts.Count > 0)
                {
                    ProgressWindow progress = new ProgressWindow(new ExecuteScripts(selectedServer));
                    progress.ShowDialog();
                }
            });
            // Url Command
            RemoveUrlCommand = new DelegateCommand(() =>
            {
                if (SelectedUrl != null)
                {
                    if (Models.Dialogs.MessageBox.Show($"Удалить \"{SelectedUrl}\" из списка?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        selectedServer.Urls.Remove(SelectedUrl);
                    }
                }
            });
            ClearUrlsCommand = new DelegateCommand(() =>
            {
                if (selectedServer != null && selectedServer.Urls.Count > 0)
                {
                    if (Models.Dialogs.MessageBox.Show("Очистить список сылок?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        selectedServer.Urls.Clear();
                    }
                }
            });
            ExportUrlsCommand = new DelegateCommand(() =>
            {
                if (selectedServer != null && selectedServer.Urls.Count > 0)
                {
                    using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog
                    {
                        Filter = "Текстовый докуент|*.txt",
                        DefaultExt = "txt"
                    })
                    {
                        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            ProgressWindow progressWindow = new ProgressWindow(new ExportUrls(selectedServer.Urls, saveFileDialog.FileName));
                            progressWindow.ShowDialog();
                        }
                    }
                }
            });
            ImportUrlsCommand = new DelegateCommand(() =>
            {
                if (selectedServer != null)
                {
                    using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
                    {
                        Filter = "Текстовый докуент|*.txt",
                        DefaultExt = "txt"
                    })
                    {
                        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            if (selectedServer.Urls.Count > 0)
                            {
                                if (Models.Dialogs.MessageBox.Show("Очистить список сылок перед добавлением?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    selectedServer.Urls.Clear();
                                }
                            }
                            ProgressWindow progressWindow = new ProgressWindow(new ImportUrls(selectedServer.Urls, openFileDialog.FileName));
                            progressWindow.ShowDialog();
                        }
                    }
                }
            });
            DownloadFilesCommand = new DelegateCommand(() =>
            {
                if (selectedServer != null && selectedServer.Urls.Count > 0)
                {
                    System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog { ShowNewFolderButton = true };
                    if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        ProgressWindow progressWindow = new ProgressWindow(
                            new Download(selectedServer.Urls, folderBrowserDialog.SelectedPath));
                        progressWindow.ShowDialog();
                    }
                }
            });
        }
    }
}