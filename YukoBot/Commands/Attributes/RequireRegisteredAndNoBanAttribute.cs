using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Linq;
using System.Threading.Tasks;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class RequireRegisteredAndNoBanAttribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            ulong userId = ctx.Member.Id;
            YukoDbContext dbContext = new YukoDbContext();
            DbUser dbUser = dbContext.Users.Find(userId);
            if (dbUser != null)
            {
                DbBan dbBan = dbContext.Bans.Where(x => x.UserId == userId && x.ServerId == ctx.Guild.Id)
                    .FirstOrDefault();
                return Task.FromResult(dbBan == null);
            }
            return Task.FromResult(false);
        }
    }
}