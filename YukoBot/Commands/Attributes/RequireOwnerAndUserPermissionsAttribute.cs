using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace YukoBot.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class RequireOwnerAndUserPermissionsAttribute : CheckBaseAttribute
    {
        public RequireOwnerAndUserPermissionsAttribute(Permissions permissions)
        {
            Permissions = permissions;
        }

        public Permissions Permissions { get; }

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            DiscordApplication currentApplication = ctx.Client.CurrentApplication;
            bool accessOwner;
            if (currentApplication == null)
            {
                DiscordUser currentUser = ctx.Client.CurrentUser;
                accessOwner = ctx.User.Id == currentUser.Id;
            }
            else
            {
                accessOwner = currentApplication.Owners.Any((DiscordUser x) => x.Id == ctx.User.Id);
            }
            Permissions permissions = ctx.Channel.PermissionsFor(ctx.Member);
            return Task.FromResult(accessOwner || permissions.HasPermission(Permissions));
        }
    }
}
