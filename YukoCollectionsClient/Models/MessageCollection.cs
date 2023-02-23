using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace YukoCollectionsClient.Models
{
    public class MessageCollection : BindableBase, IEquatable<MessageCollection>
    {
        #region Fields
        private string _name;
        private ObservableCollection<MessageCollectionItem> _items = new ObservableCollection<MessageCollectionItem>();
        private ObservableCollection<string> _urls = new ObservableCollection<string>();
        #endregion

        #region Propirties
        public ulong Id { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<MessageCollectionItem> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<string> Urls
        {
            get { return _urls; }
            set
            {
                _urls = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public bool Equals(MessageCollection other) =>
            other != null && Id.Equals(other.Id);

        public override bool Equals(object obj) =>
            Equals(obj as MessageCollection);

        public override int GetHashCode() =>
            Id.GetHashCode();
    }
}