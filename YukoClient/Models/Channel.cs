using System;
using Prism.Mvvm;

namespace YukoClient.Models
{
    public class Channel : BindableBase, IEquatable<Channel>
    {
        #region Fields
        private ulong _id;
        private string _name;
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

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public bool Equals(Channel other) => other != null && _id.Equals(other._id);
        public override bool Equals(object obj) => Equals(obj as Channel);
        public override int GetHashCode() => _id.GetHashCode();
    }
}