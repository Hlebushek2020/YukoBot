using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    [Table("collection_items")]
    public class DbCollectionItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public ulong Id { get; set; }

        [ForeignKey(nameof(Collection))]
        [Column("collection_id")]
        public ulong CollectionId { get; set; }

        public DbCollection Collection { get; set; }

        [Required]
        [Column("channel")]
        public ulong ChannelId { get; set; }

        [ForeignKey(nameof(Message))]
        [Column("message_id")]
        public ulong MessageId { get; set; }

        public DbMessage Message { get; set; }
    }
}
