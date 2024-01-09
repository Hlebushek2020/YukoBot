using System;
using Newtonsoft.Json;
using Prism.Mvvm;

namespace YukoCollectionsClient.Models
{
    public class MessageCollectionItem : BindableBase, IEquatable<MessageCollectionItem>
    {
        #region Fields
        private ulong _channelId;
        private ulong _messageId;
        private bool? _isChannelNotFound;
        private bool? _isMessageNotFound;
        #endregion

        #region Propirties
        public ulong ChannelId
        {
            get => _channelId;
            set
            {
                _channelId = value;
                RaisePropertyChanged();
            }
        }

        public ulong MessageId
        {
            get => _messageId;
            set
            {
                _messageId = value;
                RaisePropertyChanged();
            }
        }

        [JsonIgnore]
        public bool? IsChannelNotFound
        {
            get => _isChannelNotFound;
            set
            {
                _isChannelNotFound = value;
                RaisePropertyChanged();
            }
        }

        [JsonIgnore]
        public bool? IsMessageNotFound
        {
            get => _isMessageNotFound;
            set
            {
                _isMessageNotFound = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public bool Equals(MessageCollectionItem other) => other != null && _messageId == other._messageId;
        public override bool Equals(object obj) => Equals(obj as MessageCollectionItem);
        public override int GetHashCode() => _messageId.GetHashCode();
    }
}