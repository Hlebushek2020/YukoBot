using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database
{
    public class DbUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }

        [Required]
        public string Nikname { get; set; }

        [Required]
        public string Password { get; set; }

        public string Token { get; set; }

        public DateTime LoginTime { get; set; }

        public List<DbBan> Bans { get; set; }
    }
}
