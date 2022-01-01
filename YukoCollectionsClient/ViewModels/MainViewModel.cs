using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;
using YukoClientBase.Interfaces;
using YukoCollectionsClient.Models;
using YukoCollectionsClient.Models.Progress;

namespace YukoCollectionsClient.ViewModels
{
    public class MainViewModel : BindableBase, ICloseableView, IViewTitle
    {
        #region Fields
        private MessageCollection selectedMessageCollection;
        #endregion

        #region Propirties
        public string Title { get => App.Name; }
        public Action Close { get; set; }
        public ImageBrush Avatar { get { return Storage.Current.Avatar; } }
        public string Nikname { get { return Storage.Current.Nikname; } }
        public string Id { get { return Storage.Current.Id.ToString(); } }
        public ObservableCollection<MessageCollection> MessageCollections { get { return Storage.Current.MessageCollections; } }
        public MessageCollection SelectedMessageCollection
        {
            get { return selectedMessageCollection; }
            set
            {
                if (value != null)
                {
                    selectedMessageCollection = value;
                    RaisePropertyChanged("MessageCollectionItems");
                    RaisePropertyChanged("Urls");
                }
            }
        }
        public ObservableCollection<MessageCollectionItem> MessageCollectionItems { get { return selectedMessageCollection?.Items; } }
        public MessageCollectionItem SelectedMessageCollectionItem { get; set; }
        public ObservableCollection<string> Urls { get { return selectedMessageCollection?.Urls; } }
        public string SelectedUrl { get; set; }
        #endregion

        #region Commands
        public DelegateCommand WindowLoadedCommand { get; }
        // User Commands
        public DelegateCommand AppSettingsCommand { get; }
        // Message Collections Commands
        public DelegateCommand UpdateMessageCollectionsCommand { get; }
        // Message Collection Commands
        public DelegateCommand RemoveMessageCollectionItemCommand { get; }
        public DelegateCommand ExportMessageCollectionCommand { get; }
        public DelegateCommand ImportMessageCollectionCommand { get; }
        public DelegateCommand GetUrlsFromMessageCollectionCommand { get; }
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
            /*WindowLoadedCommand = new DelegateCommand(() =>
            {
                ProgressWindow progress = new ProgressWindow(new StorageInitialization());
                progress.ShowDialog();
            });*/
            // User Commands
            AppSettingsCommand = new DelegateCommand(() =>
            {
                SettingsWindow settingsWindow = new SettingsWindow();
                settingsWindow.ShowDialog();
            });
            // Message Collections Commands
            /*UpdateMessageCollectionsCommand = new DelegateCommand(() =>
            {
                MessageBoxResult messageResult = Models.Dialogs.MessageBox.Show("Перезаписать данные текущих серверов? Внимание! Это приведет к потере списка правил и ссылок.", App.Name, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (messageResult != MessageBoxResult.Cancel)
                {
                    ProgressWindow progress = new ProgressWindow(new UpdateServerList(messageResult == MessageBoxResult.Yes));
                    progress.ShowDialog();
                }
            });*/
            // Message Collection Commands
            RemoveMessageCollectionItemCommand = new DelegateCommand(() =>
            {
                if (SelectedMessageCollectionItem != null)
                {
                    if (Models.Dialogs.MessageBox.Show($"Удалить сообщение {SelectedMessageCollectionItem.MessageId} из списка?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        selectedMessageCollection.Items.Remove(SelectedMessageCollectionItem);
                    }
                }
            });
            ExportMessageCollectionCommand = new DelegateCommand(() =>
            {
                if (selectedMessageCollection != null && selectedMessageCollection.Items.Count > 0)
                {
                    using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog
                    {
                        Filter = "Yuko Script|*.yukoscript",
                        DefaultExt = "yukoscript"
                    })
                    {
                        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            ProgressWindow progressWindow = new ProgressWindow(
                                new ExportMessageCollection(selectedMessageCollection.Items, saveFileDialog.FileName));
                            progressWindow.ShowDialog();
                        }
                    }
                }
            });
            ImportMessageCollectionCommand = new DelegateCommand(() =>
            {
                if (selectedMessageCollection != null)
                {
                    using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
                    {
                        Filter = "Yuko Script|*.yukoscript",
                        DefaultExt = "yukoscript"
                    })
                    {
                        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            if (selectedMessageCollection.Items.Count > 0)
                            {
                                if (Models.Dialogs.MessageBox.Show("Очистить список правил перед добавлением?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    selectedMessageCollection.Items.Clear();
                                }
                            }
                            ProgressWindow progressWindow = new ProgressWindow(
                                new ImportMessageCollection(selectedMessageCollection.Items, openFileDialog.FileName));
                            progressWindow.ShowDialog();
                        }
                    }
                }
            });
            /*GetUrlsFromMessageCollectionCommand = new DelegateCommand(() =>
            {
                if (selectedServer != null && selectedServer.Scripts.Count > 0)
                {
                    ProgressWindow progress = new ProgressWindow(new ExecuteScripts(selectedServer));
                    progress.ShowDialog();
                }
            });*/
            // Url Command
            RemoveUrlCommand = new DelegateCommand(() =>
            {
                if (SelectedUrl != null)
                {
                    if (Models.Dialogs.MessageBox.Show($"Удалить \"{SelectedUrl}\" из списка?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        selectedMessageCollection.Urls.Remove(SelectedUrl);
                    }
                }
            });
            ClearUrlsCommand = new DelegateCommand(() =>
            {
                if (selectedMessageCollection != null && selectedMessageCollection.Urls.Count > 0)
                {
                    if (Models.Dialogs.MessageBox.Show("Очистить список сылок?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        selectedMessageCollection.Urls.Clear();
                    }
                }
            });
            ExportUrlsCommand = new DelegateCommand(() =>
            {
                if (selectedMessageCollection != null && selectedMessageCollection.Urls.Count > 0)
                {
                    using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog
                    {
                        Filter = "Текстовый докуент|*.txt",
                        DefaultExt = "txt"
                    })
                    {
                        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            ProgressWindow progressWindow = new ProgressWindow(new ExportUrls(selectedMessageCollection.Urls, saveFileDialog.FileName));
                            progressWindow.ShowDialog();
                        }
                    }
                }
            });
            ImportUrlsCommand = new DelegateCommand(() =>
            {
                if (selectedMessageCollection != null)
                {
                    using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
                    {
                        Filter = "Текстовый докуент|*.txt",
                        DefaultExt = "txt"
                    })
                    {
                        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            if (selectedMessageCollection.Urls.Count > 0)
                            {
                                if (Models.Dialogs.MessageBox.Show("Очистить список сылок перед добавлением?", App.Name, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    selectedMessageCollection.Urls.Clear();
                                }
                            }
                            ProgressWindow progressWindow = new ProgressWindow(new ImportUrls(selectedMessageCollection.Urls, openFileDialog.FileName));
                            progressWindow.ShowDialog();
                        }
                    }
                }
            });
            DownloadFilesCommand = new DelegateCommand(() =>
            {
                if (selectedMessageCollection != null && selectedMessageCollection.Urls.Count > 0)
                {
                    System.Windows.Forms.FolderBrowserDialog folderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog { ShowNewFolderButton = true };
                    if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        ProgressWindow progressWindow = new ProgressWindow(
                            new Download(selectedMessageCollection.Urls, folderBrowserDialog.SelectedPath));
                        progressWindow.ShowDialog();
                    }
                }
            });
        }
    }
}