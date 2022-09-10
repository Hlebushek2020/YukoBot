using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    [Table("messages_links")]
    [Index(nameof(MessageId), nameof(Link), IsUnique = true)]
    public class DbMessageLink
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public ulong Id { get; set; }

        [Column("message_id")]
        [ForeignKey("CollectionItem")]
        public ulong MessageId { get; set; }

        public DbCollectionItem CollectionItem { get; set; }

        [Column("link")]
        [Required]
        [MaxLength(256)]
        public string Link { get; set; }
    }
}