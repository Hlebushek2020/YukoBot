using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    /// <summary>
    /// Model describing the entity of "collections". This entity is responsible for storing information about user collections.
    /// </summary>
    [Table("collections")]
    public class DbCollection
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
        /// Collection name (must not exceed 256 characters). Required field.
        /// </summary>
        [Required]
        [MaxLength(256)]
        [Column("name")]
        public string Name { get; set; }
    }
}