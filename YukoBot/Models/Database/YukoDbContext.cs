using System;
using Microsoft.EntityFrameworkCore;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Models.Database
{
    public class YukoDbContext : DbContext, IYukoDbContext
    {
        private readonly IYukoSettings _yukoSettings;

        public DbSet<DbUser> Users { get; set; }
        public DbSet<DbBan> Bans { get; set; }
        public DbSet<DbCollection> Collections { get; set; }
        public DbSet<DbCollectionItem> CollectionItems { get; set; }
        public DbSet<DbGuildSettings> GuildsSettings { get; set; }
        public DbSet<DbMessage> Messages { get; set; }

        public YukoDbContext(IYukoSettings yukoSettings)
        {
            _yukoSettings = yukoSettings;

            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string connection =
                $"server={_yukoSettings.DatabaseHost};user={_yukoSettings.DatabaseUser};password={
                    _yukoSettings.DatabasePassword};database=YukoBot;";
            MySqlServerVersion serverVersion = new MySqlServerVersion(
                new Version(
                    _yukoSettings.DatabaseVersion[0],
                    _yukoSettings.DatabaseVersion[1],
                    _yukoSettings.DatabaseVersion[2]));
            optionsBuilder.UseMySql(connection, serverVersion);
        }
    }
}