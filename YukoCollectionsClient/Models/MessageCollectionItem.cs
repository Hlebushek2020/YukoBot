using Prism.Mvvm;
using System;

namespace YukoCollectionsClient.Models
{
    public class MessageCollectionItem : BindableBase, IEquatable<MessageCollectionItem>
    {
        #region Fields
        private ulong channelId;
        private ulong messageId;
        #endregion

        #region Propirties
        public ulong ChannelId
        {
            get { return channelId; }
            set
            {
                channelId = value;
                RaisePropertyChanged();
            }
        }

        public ulong MessageId
        {
            get { return messageId; }
            set
            {
                messageId = value;
                RaisePropertyChanged();
            }
        }
        #endregion

        public bool Equals(MessageCollectionItem other)
        {
            return other != null && messageId == other.messageId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MessageCollectionItem);
        }

        public override int GetHashCode()
        {
            return messageId.GetHashCode();
        }
    }
}