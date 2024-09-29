using DSharpPlus.CommandsNext;
using System.Collections.Generic;
using System.Threading.Tasks;
using YukoBot.Commands.Exceptions;
using YukoBot.Commands.Models;

namespace YukoBot.Commands
{
    public class CommandModule : BaseCommandModule
    {
        private static readonly Dictionary<string, Category> _registeredCategories = new Dictionary<string, Category>();

        public Category Category { get; }
        public string CommandAccessError { get; }

        protected IYukoBot Bot { get; }

        protected CommandModule(IYukoBot yukoBot, Category category, string commandAccessError)
        {
            Bot = yukoBot;

            // register category
            _registeredCategories.TryAdd(category.HelpCommand, category);

            Category = category;
            CommandAccessError = commandAccessError;
        }

        protected static bool TryGetRegisteredCategory(string helpCommand, out Category category) =>
            _registeredCategories.TryGetValue(helpCommand, out category);

        protected static IEnumerable<Category> GetCategories() => _registeredCategories.Values;

        public override Task BeforeExecutionAsync(CommandContext ctx)
        {
            if (Bot.IsShutdown)
                throw new ShutdownBotException();

            return Task.CompletedTask;
        }
    }
}