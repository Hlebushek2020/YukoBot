using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Commands.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class RequireRegisteredAndNoBanAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            ulong userId = ctx.Member.Id;
            YukoDbContext db = new YukoDbContext();
            DbUser dbUser = db.Users.Find(userId);
            if (dbUser != null)
            {
                DbBan dbBan = db.Bans.Where(x => x.UserId == userId && x.ServerId == ctx.Guild.Id).FirstOrDefault();
                return Task.FromResult(dbBan == null);
            }
            return Task.FromResult(false);
        }
    }
}