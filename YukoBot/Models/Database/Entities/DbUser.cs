using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    public class DbUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }

        [Required]
        [MaxLength(64)]
        public string Nikname { get; set; }

        [Required]
        [MaxLength(64)]
        public string Password { get; set; }

        [MaxLength(36)]
        public string Token { get; set; }

        public DateTime LoginTime { get; set; }

        public bool InfoMessages { get; set; } = true;

        //public List<DbBan> Bans { get; set; }

        //public List<DbCollection> Collections { get; set; }
    }
}