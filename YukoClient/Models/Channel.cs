using System;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace YukoClient.Models
{
    public class Channel : BindableBase, IEquatable<Channel>
    {
        private string _name;

        #region Propirties
        public ulong Id { get; }

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

        [JsonConstructor]
        public Channel(ulong id, string name)
        {
            Id = id;
            Name = name;
        }

        public bool Equals(Channel other) => other != null && Id.Equals(other.Id);
        public override bool Equals(object obj) => Equals(obj as Channel);
        public override int GetHashCode() => Id.GetHashCode();
    }
}