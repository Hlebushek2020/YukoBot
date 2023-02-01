namespace YukoBot.Models.Database.JoinedEntities
{
    public class CollectionItemJoinMessage
    {
        public ulong CollectionItemId { get; set; }
        public ulong CollectionId { get; set; }
        public ulong MessageId { get; set; }
        public bool IsSavedLinks { get; set; }
        public ulong ChannelId { get; set; }
        public string Link { get; set; }
    }
}