using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    /// <summary>
    /// Model describing the entity of "guilds_settings". This entity is responsible for storing server settings.
    /// </summary>
    [Table("guilds_settings")]
    public class DbGuildSettings
    {
        /// <summary>
        /// Primary key. The unique identifier of the entry.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public ulong Id { get; set; }

        /// <summary>
        /// Channel for searching messages for the add-by-id command. Optional field.
        /// </summary>
        [Column("art_channel_id")]
        public ulong? ArtChannelId { get; set; }

        /// <summary>
        /// Sending optional messages to the same channel where one of the following commands was executed: add,
        /// start, end. Required field. Default value: false.
        /// </summary>
        [Required]
        [Column("add_command_response")]
        public bool AddCommandResponse { get; set; } = false;

        /// <summary>
        /// Gets or sets the channel ID for system messages (such as disabling and enabling a bot).
        /// </summary>
        [Column("notification_channel_id")]
        public ulong? NotificationChannelId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether notifications about the inclusion of the bot should be received or
        /// not. Required field. Default value: false.
        /// </summary>
        [Required]
        [Column("ready_notification")]
        public bool IsReadyNotification { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether bot shutdown notifications should be received or not. Required
        /// field. Default value: false.
        /// </summary>
        [Required]
        [Column("shutdown_notification")]
        public bool IsShutdownNotification { get; set; } = false;
    }
}