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
        private ulong id = 0;
        private string iconUri;
        private string name;
        private ObservableCollection<Channel> channels;
        private ObservableCollection<Script> scripts = new ObservableCollection<Script>();
        private ObservableCollection<string> urls = new ObservableCollection<string>();
        #endregion

        #region Propirties
        public ulong Id
        {
            get { return id; }
            set
            {
                id = value;
                RaisePropertyChanged();
            }
        }

        public string IconUri
        {
            get { return iconUri; }
            set
            {
                iconUri = value;
                RaisePropertyChanged("Icon");
            }
        }

        [JsonIgnore]
        public ImageBrush Icon
        {
            get
            {
                if (string.IsNullOrEmpty(iconUri))
                {
                    return null;
                }
                return new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(iconUri, UriKind.Absolute)),
                    Stretch = Stretch.UniformToFill
                };
            }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Channel> Channels
        {
            get { return channels; }
            set
            {
                channels = value;
                RaisePropertyChanged();
            }
        }

        [JsonIgnore]
        public ObservableCollection<Script> Scripts
        {
            get { return scripts; }
            set
            {
                scripts = value;
                RaisePropertyChanged();
            }
        }

        [JsonIgnore]
        public ObservableCollection<string> Urls
        {
            get { return urls; }
            set
            {
                urls = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public bool Equals(Server other)
        {
            return other == null ? false : id.Equals(other.id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Server);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}