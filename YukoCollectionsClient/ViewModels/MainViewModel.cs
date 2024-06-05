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
using MessageBox = Sergey.UI.Extension.Dialogs.MessageBox;
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
        public string SelectedUrl { get; set; }
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
                                messageBoxResult == MessageBoxResult.Yes), true);
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
            GetUrlsFromMessageCollectionCommand = new DelegateCommand(() =>
            {
                if (_selectedMessageCollection != null && _selectedMessageCollection.Items.Count > 0)
                {
                    if (_selectedMessageCollection.Urls.Count != 0 &&
                        SUI.Dialogs.MessageBox.Show("Очистить список ссылок перед добавлением?", App.Name,
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedMessageCollection.Urls.Clear();
                    }

                    ProgressWindow progress =
                        new ProgressWindow(Title, new GetUrlsFromMessageCollection(_selectedMessageCollection), true);
                    progress.ShowDialog();
                }
            });
            // Url Command
            RemoveUrlCommand = new DelegateCommand(() =>
            {
                if (SelectedUrl != null)
                {
                    if (SUI.Dialogs.MessageBox.Show($"Удалить \"{SelectedUrl}\" из списка?", App.Name,
                            MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedMessageCollection.Urls.Remove(SelectedUrl);
                    }
                }
            });
            ClearUrlsCommand = new DelegateCommand(() =>
            {
                if (_selectedMessageCollection != null && _selectedMessageCollection.Urls.Count > 0)
                {
                    if (SUI.Dialogs.MessageBox.Show("Очистить список сылок?", App.Name, MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        _selectedMessageCollection.Urls.Clear();
                    }
                }
            });
            ExportUrlsCommand = new DelegateCommand(() =>
            {
                if (_selectedMessageCollection != null && _selectedMessageCollection.Urls.Count > 0)
                {
                    using (System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog
                           {
                               Filter = "Текстовый докуент|*.txt", DefaultExt = "txt"
                           })
                    {
                        if (saveFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            ProgressWindow progressWindow =
                                new ProgressWindow(Title, new ExportUrls(_selectedMessageCollection.Urls,
                                    saveFileDialog.FileName));
                            progressWindow.ShowDialog();
                        }
                    }
                }
            });
            ImportUrlsCommand = new DelegateCommand(() =>
            {
                if (_selectedMessageCollection != null)
                {
                    using (System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog
                           {
                               Filter = "Текстовый докуент|*.txt", DefaultExt = "txt"
                           })
                    {
                        if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            if (_selectedMessageCollection.Urls.Count > 0)
                            {
                                if (SUI.Dialogs.MessageBox.Show("Очистить список сылок перед добавлением?", App.Name,
                                        MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    _selectedMessageCollection.Urls.Clear();
                                }
                            }

                            ProgressWindow progressWindow =
                                new ProgressWindow(Title, new ImportUrls(_selectedMessageCollection.Urls,
                                    openFileDialog.FileName));
                            progressWindow.ShowDialog();
                        }
                    }
                }
            });
            DownloadFilesCommand = new DelegateCommand(() =>
            {
                if (_selectedMessageCollection != null && _selectedMessageCollection.Urls.Count > 0)
                {
                    System.Windows.Forms.FolderBrowserDialog folderBrowserDialog =
                        new System.Windows.Forms.FolderBrowserDialog { ShowNewFolderButton = true };
                    if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        ProgressWindow progressWindow = new ProgressWindow(Title,
                            new Download(_selectedMessageCollection.Urls, folderBrowserDialog.SelectedPath), true);
                        progressWindow.ShowDialog();
                    }
                }
            });
        }

        private bool MessageCollectionsFilter(object item)
        {
            return string.IsNullOrEmpty(_searchCollections) ||
                   ((MessageCollection)item).Name.ToLower().Contains(_searchCollections);
        }
    }
}