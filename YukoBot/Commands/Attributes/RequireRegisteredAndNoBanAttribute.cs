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
        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            ulong userId = ctx.Member.Id;
            IYukoSettings settings = ctx.Services.GetService<IYukoSettings>();
            await using YukoDbContext dbContext = new YukoDbContext(settings);
            DbUser dbUser = await dbContext.Users.FindAsync(userId);

            if (dbUser == null)
                return false;

            DbBan dbBan = dbContext.Bans.FirstOrDefault(x => x.UserId == userId && x.ServerId == ctx.Guild.Id);
            return dbBan == null;
        }
    }
}