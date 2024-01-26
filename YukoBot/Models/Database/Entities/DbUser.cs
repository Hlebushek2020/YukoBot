using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    /// <summary>
    /// Model describing the entity of "users". This entity is responsible for storing information about the registered user.
    /// </summary>
    [Table("users")]
    public class DbUser
    {
        /// <summary>
        /// Primary key. The unique identifier of the entry. This identifier is taken from the corresponding property
        /// of the <see cref="DSharpPlus.Entities.DiscordUser"/> class.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public ulong Id { get; set; }

        /// <summary>
        /// Username. Required field.
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("nickname")]
        public string Username { get; set; }

        /// <summary>
        /// Hash of the user's password. Required field.
        /// </summary>
        [Required]
        [MaxLength(64)]
        [Column("password")]
        public string Password { get; set; }

        /// <summary>
        /// Date of registration. Required field.
        /// </summary>
        [Required]
        [Column("registered")]
        public DateTime Registered { get; set; }

        /// <summary>
        /// The time and date of the last login to the application. Default value: null.
        /// </summary>
        [Column("last_login")]
        public DateTime? LastLogin { get; set; }

        /// <summary>
        /// Sending optional messages to private messages with the following commands: add, start, end. Default value: true.
        /// </summary>
        [Required]
        [Column("info_messages")]
        public bool InfoMessages { get; set; } = true;

        /// <summary>
        /// Expiration date of premium access. Default value: null.
        /// </summary>
        [Column("premium_access_expires")]
        public DateTime? PremiumAccessExpires { get; set; }

        [Required]
        [Column("two_factor_authentication")]
        public bool TwoFactorAuthentication { get; set; } = true;

        [Column("refresh_token")]
        [MaxLength(64)]
        public string RefreshToken { get; set; }

        /// <summary>
        /// Indicates whether the user has premium access or not. Isn't column.
        /// </summary>
        [NotMapped]
        public bool HasPremiumAccess => PremiumAccessExpires.HasValue && DateTime.Now <= PremiumAccessExpires.Value;
    }
}