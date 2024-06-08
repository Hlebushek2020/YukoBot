using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using YukoClientBase.Interfaces;
using YukoClientBase.Views;
using YukoCollectionsClient.Models;
using YukoCollectionsClient.Models.Progresses;
using MessageBox = YukoClientBase.Dialogs.MessageBox;
using FolderBrowserDialog = System.Windows.Forms.FolderBrowserDialog;
using DialogResult = System.Windows.Forms.DialogResult;
using SaveFileDialog = System.Windows.Forms.SaveFileDialog;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

namespace YukoCollectionsClient.ViewModels
{
    public class MainViewModel : BindableBase, IFullscreenEvent
    {
        #region Fields
        private MessageCollection _selectedMessageCollection;
        private MessageCollectionItem _selectedMessageCollectionItem;
        private string _searchCollections;
        private string _selectedUrl;
        #endregion

        #region Propirties
        public string Title => App.Name;
        public Action Close { get; set; }
        public ImageBrush Avatar => Storage.Current.Avatar;
        public string Username => Storage.Current.Username;
        public string UserId => Storage.Current.UserId.ToString();
        public ObservableCollection<MessageCollection> MessageCollections => Storage.Current.MessageCollections;

        public MessageCollection SelectedMessageCollection
        {
            get => _selectedMessageCollection;
            set
            {
                _selectedMessageCollection = value;

                RaisePropertyChanged(nameof(MessageCollectionItems));
                RaisePropertyChanged(nameof(Urls));

                RemoveMessageCollectionItemCommand.RaiseCanExecuteChanged();
                ExportMessageCollectionCommand.RaiseCanExecuteChanged();
                ImportMessageCollectionCommand.RaiseCanExecuteChanged();

                GetUrlsFromMessageCollectionCommand.RaiseCanExecuteChanged();
                ClearUrlsCommand.RaiseCanExecuteChanged();
                ExportUrlsCommand.RaiseCanExecuteChanged();
                ImportUrlsCommand.RaiseCanExecuteChanged();
                DownloadFilesCommand.RaiseCanExecuteChanged();
            }
        }

        public string SearchCollections
        {
            get => _searchCollections;
            set
            {
                _searchCollections = value.ToLower();
                CollectionViewSource.GetDefaultView(MessageCollections).Refresh();
            }
        }

        public ObservableCollection<MessageCollectionItem> MessageCollectionItems => _selectedMessageCollection?.Items;

        public MessageCollectionItem SelectedMessageCollectionItem
        {
            get => _selectedMessageCollectionItem;
            set
            {
                _selectedMessageCollectionItem = value;
                RaisePropertyChanged();
                RemoveMessageCollectionItemCommand.RaiseCanExecuteChanged();
            }
        }

        public ObservableCollection<string> Urls => _selectedMessageCollection?.Urls;

        public string SelectedUrl
        {
            get => _selectedUrl;
            set
            {
                _selectedUrl = value;
                RaisePropertyChanged();
                RemoveUrlCommand.RaiseCanExecuteChanged();
            }
        }
        #endregion

        #region Commands
        public DelegateCommand FullscreenCommand { get; }
        public DelegateCommand WindowLoadedCommand { get; }

        // User Commands
        public DelegateCommand AppSettingsCommand { get; }

        // Message Collections Commands
        public DelegateCommand UpdateMessageCollectionsCommand { get; }
        public DelegateCommand DownloadAllCollectionsCommand { get; }

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

        public event FullscreenEventHandler FullscreenEvent;

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public MainViewModel()
        {
            Storage.Current.PropertyChanged += (s, e) => { RaisePropertyChanged(e.PropertyName); };

            FullscreenCommand = new DelegateCommand(() => FullscreenEvent?.Invoke());
            WindowLoadedCommand = new DelegateCommand(
                () =>
                {
                    ProgressWindow progress = new ProgressWindow(Title, new UpdateMessageCollections(true));
                    progress.ShowDialog();
                    CollectionViewSource.GetDefaultView(MessageCollections).Filter = MessageCollectionsFilter;
                });

            // User Commands
            AppSettingsCommand = new DelegateCommand(
                () =>
                {
                    SettingsWindow settingsWindow = new SettingsWindow(App.Name);
                    settingsWindow.ShowDialog();
                });

            // Message Collections Commands
            UpdateMessageCollectionsCommand = new DelegateCommand(
                () =>
                {
                    MessageBoxResult messageResult = MessageBox.Show(
                        "Перезаписать данные текущих коллекций (быстрее)? Внимание! Это приведет к потере списка ссылок.",
                        App.Name, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

                    if (messageResult == MessageBoxResult.Cancel)
                        return;

                    bool overrideMessageCollections = messageResult == MessageBoxResult.Yes;
                    ProgressWindow progress =
                        new ProgressWindow(Title, new UpdateMessageCollections(overrideMessageCollections));
                    progress.ShowDialog();

                    if (overrideMessageCollections)
                        CollectionViewSource.GetDefaultView(MessageCollections).Filter = MessageCollectionsFilter;

                    DownloadAllCollectionsCommand.RaiseCanExecuteChanged();
                });
            DownloadAllCollectionsCommand = new DelegateCommand(
                () =>
                {
                    using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                    {
                        folderBrowserDialog.ShowNewFolderButton = true;

                        if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                            return;

                        MessageBoxResult messageBoxResult = MessageBox.Show(
                            "Очищать список ссылок коллекции перед добавлением?", App.Name, MessageBoxButton.YesNo,
                            MessageBoxImage.Question);
                        ProgressWindow progressWindow = new ProgressWindow(Title,
                            new DownloadAll(MessageCollections, folderBrowserDialog.SelectedPath,
                                messageBoxResult == MessageBoxResult.Yes));
                        progressWindow.ShowDialog();
                    }
                },
                () => MessageCollections != null && MessageCollections.Count != 0);

            // Message Collection Commands
            RemoveMessageCollectionItemCommand = new DelegateCommand(
                () =>
                {
                    if (MessageBox.Show(
                            $"Удалить сообщение {SelectedMessageCollectionItem.MessageId} из списка?", App.Name,
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedMessageCollection.Items.Remove(SelectedMessageCollectionItem);
                    }
                },
                () => _selectedMessageCollection != null && SelectedMessageCollectionItem != null);
            ExportMessageCollectionCommand = new DelegateCommand(
                () =>
                {
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.DefaultExt = "json";
                        saveFileDialog.Filter = "JavaScript Object Notation|*.json";

                        if (saveFileDialog.ShowDialog() != DialogResult.OK)
                            return;

                        ProgressWindow progressWindow = new ProgressWindow(Title,
                            new ExportMessageCollection(_selectedMessageCollection.Items,
                                saveFileDialog.FileName));
                        progressWindow.ShowDialog();
                    }
                }, () => _selectedMessageCollection != null && _selectedMessageCollection.Items.Count > 0);
            ImportMessageCollectionCommand = new DelegateCommand(
                () =>
                {
                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.DefaultExt = "json";
                        openFileDialog.Filter = "JavaScript Object Notation|*.json";
                        if (openFileDialog.ShowDialog() != DialogResult.OK)
                            return;

                        if (_selectedMessageCollection.Items.Count > 0)
                        {
                            if (MessageBox.Show("Очистить список правил перед добавлением?", App.Name,
                                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                _selectedMessageCollection.Items.Clear();
                            }
                        }

                        ProgressWindow progressWindow = new ProgressWindow(Title,
                            new ImportMessageCollection(_selectedMessageCollection.Items, openFileDialog.FileName));
                        progressWindow.ShowDialog();
                    }
                },
                () => _selectedMessageCollection != null);
            GetUrlsFromMessageCollectionCommand = new DelegateCommand(
                () =>
                {
                    if (_selectedMessageCollection.Urls.Count != 0 &&
                        MessageBox.Show("Очистить список ссылок перед добавлением?", App.Name,
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedMessageCollection.Urls.Clear();
                    }

                    ProgressWindow progress = new ProgressWindow(Title,
                        new GetUrlsFromMessageCollection(_selectedMessageCollection));
                    progress.ShowDialog();
                },
                () => _selectedMessageCollection != null);

            // Url Command
            RemoveUrlCommand = new DelegateCommand(
                () =>
                {
                    if (MessageBox.Show($"Удалить \"{SelectedUrl}\" из списка?", App.Name,
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedMessageCollection.Urls.Remove(SelectedUrl);
                    }
                },
                () => _selectedUrl != null);
            ClearUrlsCommand = new DelegateCommand(
                () =>
                {
                    if (MessageBox.Show("Очистить список сылок?", App.Name, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedMessageCollection.Urls.Clear();
                    }
                },
                () => _selectedMessageCollection != null && _selectedMessageCollection.Urls.Count > 0);
            ExportUrlsCommand = new DelegateCommand(
                () =>
                {
                    using (SaveFileDialog saveFileDialog = new SaveFileDialog())
                    {
                        saveFileDialog.Filter = "Текстовый докуент|*.txt";
                        saveFileDialog.DefaultExt = "txt";

                        if (saveFileDialog.ShowDialog() != DialogResult.OK)
                            return;

                        ProgressWindow progressWindow = new ProgressWindow(Title,
                            new ExportUrls(_selectedMessageCollection.Urls, saveFileDialog.FileName));
                        progressWindow.ShowDialog();
                    }
                },
                () => _selectedMessageCollection != null);
            ImportUrlsCommand = new DelegateCommand(
                () =>
                {
                    using (OpenFileDialog openFileDialog = new OpenFileDialog())
                    {
                        openFileDialog.Filter = "Текстовый докуент|*.txt";
                        openFileDialog.DefaultExt = "txt";

                        if (openFileDialog.ShowDialog() != DialogResult.OK)
                            return;

                        if (_selectedMessageCollection.Urls.Count > 0)
                        {
                            if (MessageBox.Show("Очистить список сылок перед добавлением?", App.Name,
                                    MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                            {
                                _selectedMessageCollection.Urls.Clear();
                            }
                        }

                        ProgressWindow progressWindow = new ProgressWindow(Title,
                            new ImportUrls(_selectedMessageCollection.Urls, openFileDialog.FileName));
                        progressWindow.ShowDialog();
                    }
                },
                () => _selectedMessageCollection != null);
            DownloadFilesCommand = new DelegateCommand(
                () =>
                {
                    using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                    {
                        folderBrowserDialog.ShowNewFolderButton = true;

                        if (folderBrowserDialog.ShowDialog() != DialogResult.OK)
                            return;

                        ProgressWindow progressWindow = new ProgressWindow(Title,
                            new Download(_selectedMessageCollection.Urls, folderBrowserDialog.SelectedPath));
                        progressWindow.ShowDialog();
                    }
                },
                () => _selectedMessageCollection != null);
        }

        private bool MessageCollectionsFilter(object item)
        {
            return string.IsNullOrEmpty(_searchCollections) ||
                ((MessageCollection)item).Name.ToLower().Contains(_searchCollections);
        }
    }
}