using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    public class DbCollection
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public ulong Id { get; set; }

        [ForeignKey("User")]
        public ulong UserId { get; set; }

        public DbUser User { get; set; }

        [Required]
        [MaxLength(256)]
        public string Name { get; set; }

        public List<DbCollectionItem> CollectionItems { get; set; }

    }
}
