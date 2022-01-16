using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace YukoCollectionsClient.Models
{
    public class MessageCollection : BindableBase, IEquatable<MessageCollection>
    {
        #region Fields
        private string name;
        private ObservableCollection<MessageCollectionItem> items = new ObservableCollection<MessageCollectionItem>();
        private ObservableCollection<string> urls = new ObservableCollection<string>();
        #endregion

        #region Propirties
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<MessageCollectionItem> Items
        {
            get { return items; }
            set
            {
                items = value;
                RaisePropertyChanged();
            }
        }

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

        public bool Equals(MessageCollection other)
        {
            return other != null && name.Equals(other.name);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MessageCollection);
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }
    }
}