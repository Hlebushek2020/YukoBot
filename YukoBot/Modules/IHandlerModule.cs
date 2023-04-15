using DSharpPlus;
using System.Threading.Tasks;

namespace YukoBot.Modules
{
    internal interface IHandlerModule<THandlerArgs>
    {
        Task Handler(DiscordClient sender, THandlerArgs e);
    }
}