using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YukoBot.Models.Database.Entities
{
    [Table("guilds_settings")]
    public class DbGuildSettings
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column("id")]
        public ulong Id { get; set; }

        [Column("art_channel")]
        public ulong? ArtChannelId { get; set; }

        [Column("add_command_response")]
        public bool AddCommandResponse { get; set; } = true;
    }
}
