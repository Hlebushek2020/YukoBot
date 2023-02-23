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

namespace YukoCollectionsClient.Models
{
    public class Storage : BindableBase, IUser
    {
        #region Fields
        private ulong _id;
        private string _avatarUri;
        private string _nikname;
        private ObservableCollection<MessageCollection> _messageCollections =
            new ObservableCollection<MessageCollection>();
        #endregion

        #region Propirties
        public static Storage Current { get; } = new Storage();

        public ulong Id
        {
            get { return _id; }
            set
            {
                _id = value;
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

        [JsonIgnore]
        public ImageBrush Avatar
        {
            get
            {
                if (string.IsNullOrEmpty(_avatarUri))
                {
                    return null;
                }
                return new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(_avatarUri, UriKind.Absolute)),
                    Stretch = Stretch.UniformToFill
                };
            }
        }

        public string Nikname
        {
            get { return _nikname; }
            set
            {
                _nikname = value;
                RaisePropertyChanged();
            }
        }

        [JsonIgnore]
        public ObservableCollection<MessageCollection> MessageCollections
        {
            get { return _messageCollections; }
            set
            {
                _messageCollections = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public void Save()
        {
            string profileFile = Path.Combine(Settings.ProgramResourceFolder, "profile.json");
            Directory.CreateDirectory(Settings.ProgramResourceFolder);
            using (StreamWriter streamWriter = new StreamWriter(profileFile, false, Encoding.UTF8))
            {
                streamWriter.Write(JsonConvert.SerializeObject(this));
            }
        }
    }
}