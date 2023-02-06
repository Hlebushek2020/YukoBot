using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    /// <summary>
    /// Model describing the entity of "messages". This entity is responsible for storing information about the
    /// messages of the collections.
    /// </summary>
    [Table("messages")]
    public class DbMessage
    {
        /// <summary>
        /// Primary key. The unique identifier of the entry. This identifier is taken from the corresponding property
        /// of the <see cref="DSharpPlus.Entities.DiscordMessage"/> class.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public ulong Id { get; set; }

        /// <summary>
        /// The unique channel identifier for the given message. This identifier is taken from the corresponding
        /// property of the <see cref="DSharpPlus.Entities.DiscordChannel"/> class. Required field.
        /// </summary>
        [Required]
        [Column("channel_id")]
        public ulong ChannelId { get; set; }

        /// <summary>
        /// List of links to attachments from this message (links are separated ';')
        /// </summary>
        [Required]
        [Column("links")]
        public string Link { get; set; }
    }
}