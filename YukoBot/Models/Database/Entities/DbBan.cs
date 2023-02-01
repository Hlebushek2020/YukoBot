using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    [Table("bans")]
    public class DbBan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public ulong Id { get; set; }

        [ForeignKey(nameof(User))]
        [Column("user_id")]
        public ulong UserId { get; set; }

        public DbUser User { get; set; }

        [Required]
        [Column("server_id")]
        public ulong ServerId { get; set; }

        [MaxLength(256)]
        [Column("reason")]
        public string Reason { get; set; }
    }
}
