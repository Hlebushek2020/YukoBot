using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    [Table("messages")]
    public class DbMessage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public ulong Id { get; set; }

        [Required]
        [Column("channel_id")]
        public ulong ChannelId { get; set; }

        [Column("links")]
        [Required]
        public string Link { get; set; }
    }
}