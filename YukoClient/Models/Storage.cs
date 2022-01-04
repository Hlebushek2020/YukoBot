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
        private ulong id;
        private string avatarUri;
        private string nikname;
        private ObservableCollection<Server> servers;
        #endregion

        #region Propirties
        public static Storage Current { get; } = new Storage();

        public ulong Id
        {
            get { return id; }
            set
            {
                id = value;
                RaisePropertyChanged();
            }
        }

        public string AvatarUri
        {
            get { return avatarUri; }
            set
            {
                avatarUri = value;
                RaisePropertyChanged("Avatar");
            }
        }

        public ImageBrush Avatar
        {
            get
            {
                if (string.IsNullOrEmpty(avatarUri))
                {
                    return null;
                }
                return new ImageBrush
                {
                    ImageSource = new BitmapImage(new Uri(avatarUri, UriKind.Absolute)),
                    Stretch = Stretch.UniformToFill
                };
            }
        }

        public string Nikname
        {
            get { return nikname; }
            set
            {
                nikname = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<Server> Servers
        {
            get { return servers; }
            set
            {
                servers = value;
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