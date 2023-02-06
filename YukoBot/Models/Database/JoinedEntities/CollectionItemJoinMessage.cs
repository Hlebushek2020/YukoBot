namespace YukoBot.Models.Database.JoinedEntities
{
    /// <summary>
    /// Contains properties describing the union of the <see cref="Models.Database.Entities.DbCollectionItem"/>
    /// and <see cref="Models.Database.Entities.DbMessage"/> entities.
    /// </summary>
    public class CollectionItemJoinMessage
    {
        /// <summary>
        /// Unique identifier of the collection item.
        /// </summary>
        public ulong CollectionItemId { get; set; }

        /// <summary>
        /// Unique identifier of the collection.
        /// </summary>
        public ulong CollectionId { get; set; }

        /// <summary>
        /// Unique identifier of the message.
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        /// See corresponding property in <see cref="Models.Database.Entities.DbCollectionItem"/>.
        /// </summary>
        public bool IsSavedLinks { get; set; }

        /// <summary>
        /// See corresponding property in <see cref="Models.Database.Entities.DbMessage"/>.
        /// </summary>
        public ulong ChannelId { get; set; }

        /// <summary>
        /// See corresponding property in <see cref="Models.Database.Entities.DbMessage"/>.
        /// </summary>
        public string Link { get; set; }
    }
}