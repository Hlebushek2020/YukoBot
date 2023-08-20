using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
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
            YukoDbContext dbContext = ctx.Services.GetService<YukoDbContext>();
            DbUser dbUser = dbContext.Users.Find(userId);
            if (dbUser != null)
            {
                DbBan dbBan = dbContext.Bans.FirstOrDefault(x => x.UserId == userId && x.ServerId == ctx.Guild.Id);
                return Task.FromResult(dbBan == null);
            }
            return Task.FromResult(false);
        }
    }
}