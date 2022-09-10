using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    [Table("collections")]
    public class DbCollection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id")]
        public ulong Id { get; set; }

        [ForeignKey("User")]
        [Column("user_id")]
        public ulong UserId { get; set; }

        public DbUser User { get; set; }

        [Required]
        [MaxLength(256)]
        [Column("name")]
        public string Name { get; set; }
    }
}