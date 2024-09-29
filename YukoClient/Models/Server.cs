using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace YukoClient.Models
{
    public class Server : BindableBase, IEquatable<Server>
    {
        #region Fields
        private ulong _id;
        private string _iconUri;
        private string _name;
        private ObservableCollection<Channel> _channels;
        private ObservableCollection<Script> _scripts = new ObservableCollection<Script>();
        private ObservableCollection<string> _urls = new ObservableCollection<string>();
        #endregion

        #region Propirties
        public ulong Id
        {
            get => _id;
            set
            {
                _id = value;
                RaisePropertyChanged();
            }
        }

        public string IconUri
        {
            get => _iconUri;
            set
            {
                _iconUri = value;
                RaisePropertyChanged(nameof(Icon));
            }
        }

        [JsonIgnore]
        public ImageBrush Icon
        {
            get
            {
                if (string.IsNullOrEmpty(_iconUri))
                    return null;

                return new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(_iconUri, UriKind.Absolute)),
                    Stretch = Stretch.UniformToFill
                };
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Channel> Channels
        {
            get => _channels;
            set
            {
                _channels = value;
                RaisePropertyChanged();
            }
        }

        [JsonIgnore]
        public ObservableCollection<Script> Scripts
        {
            get => _scripts;
            set
            {
                _scripts = value;
                RaisePropertyChanged();
            }
        }

        [JsonIgnore]
        public ObservableCollection<string> Urls
        {
            get => _urls;
            set
            {
                _urls = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public bool Equals(Server other) => other != null && _id.Equals(other._id);
        public override bool Equals(object obj) => Equals(obj as Server);
        public override int GetHashCode() => _id.GetHashCode();
    }
}