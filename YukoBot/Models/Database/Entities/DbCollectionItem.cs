using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    /// <summary>
    /// Model describing the entity of "collection_items". This entity is responsible for storing information about
    /// the elements of the collections.
    /// </summary>
    [Table("collection_items")]
    public class DbCollectionItem
    {
        /// <summary>
        /// Primary key. The unique identifier of the entry.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public ulong Id { get; set; }

        /// <summary>
        /// Foreign key. The unique identifier of the collection associated with this entry.
        /// </summary>
        [ForeignKey(nameof(Collection))]
        [Column("collection_id")]
        public ulong CollectionId { get; set; }

        /// <summary>
        /// Collection entry associated with this entry. Is not a column. Optional field.
        /// </summary>
        public DbCollection Collection { get; set; }

        /// <summary>
        /// Foreign key. The unique identifier of the message associated with this entry.
        /// </summary>
        [ForeignKey(nameof(Message))]
        [Column("message_id")]
        public ulong MessageId { get; set; }

        /// <summary>
        /// Message entry associated with this entry. Is not a column. Optional field.
        /// </summary>
        public DbMessage Message { get; set; }

        /// <summary>
        /// Indicates whether the corresponding user had premium access when the message was added to the collection.
        /// Required field.
        /// </summary>
        [Required]
        [Column("is_saved_links")]
        public bool IsSavedLinks { get; set; }
    }
}