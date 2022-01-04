using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    public class DbBan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        [ForeignKey("User")]
        public ulong UserId { get; set; }

        public DbUser User { get; set; }

        [Required]
        public ulong ServerId { get; set; }

        [MaxLength(200)]
        public string Reason { get; set; }
    }
}
