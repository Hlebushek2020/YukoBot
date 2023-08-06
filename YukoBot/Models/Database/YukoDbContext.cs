using System;
using Microsoft.EntityFrameworkCore;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Models.Database
{
    public class YukoDbContext : DbContext
    {
        public DbSet<DbUser> Users { get; set; }
        public DbSet<DbBan> Bans { get; set; }
        public DbSet<DbCollection> Collections { get; set; }
        public DbSet<DbCollectionItem> CollectionItems { get; set; }
        public DbSet<DbGuildSettings> GuildsSettings { get; set; }
        public DbSet<DbMessage> Messages { get; set; }

        public YukoDbContext() { Database.EnsureCreated(); }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IYukoSettings settings = YukoSettings.Current;

            string connection =
                $"server={settings.DatabaseHost};user={settings.DatabaseUser};password={settings.DatabasePassword
                };database=YukoBot;";
            MySqlServerVersion serverVersion = new MySqlServerVersion(
                new Version(settings.DatabaseVersion[0], settings.DatabaseVersion[1], settings.DatabaseVersion[2]));
            optionsBuilder.UseMySql(connection, serverVersion);
        }
    }
}