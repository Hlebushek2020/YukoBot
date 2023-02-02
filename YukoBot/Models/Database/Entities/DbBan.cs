using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    /// <summary>
    /// Model describing the entity of "bans". This entity is responsible for storing information about banned users.
    /// </summary>
    [Table("bans")]
    public class DbBan
    {
        /// <summary>
        /// Primary key. The unique identifier of the entry.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public ulong Id { get; set; }

        /// <summary>
        /// Foreign key. The unique identifier of the user associated with this entry.
        /// </summary>
        [ForeignKey(nameof(User))]
        [Column("user_id")]
        public ulong UserId { get; set; }

        /// <summary>
        /// User entry associated with this entry. Is not a column. Optional field.
        /// </summary>
        public DbUser User { get; set; }

        /// <summary>
        /// Unique identifier of the server associated with this entry. This identifier is taken from the
        /// corresponding property of the <see cref="DSharpPlus.Entities.DiscordChannel"/> class. Required field.
        /// </summary>
        [Required]
        [Column("server_id")]
        public ulong ServerId { get; set; }

        /// <summary>
        /// Reason for blocking a user on a given server (must not exceed 256 characters). Optional field.
        /// </summary>
        [MaxLength(256)]
        [Column("reason")]
        public string Reason { get; set; }
    }
}