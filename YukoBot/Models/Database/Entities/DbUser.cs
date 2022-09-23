using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    [Table("users")]
    public class DbUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public ulong Id { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("nikname")]
        public string Nikname { get; set; }

        [Required]
        [MaxLength(64)]
        [Column("password")]
        public string Password { get; set; }

        [MaxLength(36)]
        [Column("token")]
        public string Token { get; set; }

        [Column("login_time")]
        public DateTime? LoginTime { get; set; }

        [Column("info_messages")]
        public bool InfoMessages { get; set; } = true;

        [Column("has_premium")]
        public bool HasPremium { get; set; } = false;
    }
}