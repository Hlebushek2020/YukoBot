using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    public class DbGuildSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; set; }

        public ulong? ArtChannelId { get; set; }

        public bool AddCommandResponse { get; set; } = true;
    }
}
