using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using YukoClientBase.Interfaces;
using YukoClientBase.Models;

namespace YukoClient.Models
{
    public class Storage : BindableBase, IUser
    {
        #region Fields
        private ulong _userId;
        private string _avatarUri;
        private string _username;
        private ObservableCollection<Server> _servers;
        #endregion

        #region Propirties
        public static Storage Current { get; } = new Storage();

        public ulong UserId
        {
            get => _userId;
            set
            {
                _userId = value;
                RaisePropertyChanged();
            }
        }

        public string AvatarUri
        {
            get { return _avatarUri; }
            set
            {
                _avatarUri = value;
                RaisePropertyChanged(nameof(Avatar));
            }
        }

        public ImageBrush Avatar
        {
            get
            {
                if (string.IsNullOrEmpty(_avatarUri))
                    return null;

                return new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(_avatarUri, UriKind.Absolute)),
                    Stretch = Stretch.UniformToFill
                };
            }
        }

        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Server> Servers
        {
            get { return _servers; }
            set
            {
                _servers = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public void Save()
        {
            string profileFile = Path.Combine(Settings.ProgramResourceFolder, Settings.ServersCacheFile);
            Directory.CreateDirectory(Settings.ProgramResourceFolder);
            using (StreamWriter streamWriter = new StreamWriter(profileFile, false, Encoding.UTF8))
            {
                streamWriter.Write(JsonConvert.SerializeObject(Servers));
            }
        }
    }
}