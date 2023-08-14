using Microsoft.EntityFrameworkCore;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Models.Database
{
    internal interface IYukoDbContext
    {
        DbSet<DbUser> Users { get; }
        DbSet<DbBan> Bans { get; }
        DbSet<DbCollection> Collections { get; }
        DbSet<DbCollectionItem> CollectionItems { get; }
        DbSet<DbGuildSettings> GuildsSettings { get; }
        DbSet<DbMessage> Messages { get; }
    }
}