using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.DependencyInjection;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RequireRegisteredAttribute : CheckBaseAttribute
    {
        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            ulong userId = ctx.Member.Id;
            YukoDbContext dbContext = ctx.Services.GetService<YukoDbContext>();
            DbUser dbUser = await dbContext.Users.FindAsync(userId);
            return dbUser != null;
        }
    }
}