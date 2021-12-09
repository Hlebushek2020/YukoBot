using Prism.Mvvm;
using System;

namespace YukoClient.Models
{
    public class Channel : BindableBase, IEquatable<Channel>
    {
        #region Fields
        private ulong id;
        private string name;
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

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public bool Equals(Channel other)
        {
            return other == null ? false : id.Equals(other.id);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Channel);
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }
    }
}