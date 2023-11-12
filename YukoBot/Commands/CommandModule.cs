using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using YukoBot.Commands.Exceptions;
using YukoBot.Commands.Models;

namespace YukoBot.Commands
{
    public class CommandModule : BaseCommandModule
    {
        private static readonly Dictionary<string, Category> _registeredСategories = new Dictionary<string, Category>();

        public Category Category { get; }
        public string CommandAccessError { get; }

        protected IYukoBot Bot { get; }

        protected CommandModule(IYukoBot yukoBot, Category category, string commandAccessError)
        {
            Bot = yukoBot;

            // register category
            _registeredСategories.TryAdd(category.HelpCommand, category);

            Category = category;
            CommandAccessError = commandAccessError;
        }

        protected static bool TryGetRegisteredСategory(string helpCommand, out Category category) =>
            _registeredСategories.TryGetValue(helpCommand, out category);

        protected static IEnumerable<Category> GetCategories() => _registeredСategories.Values;

        public override Task BeforeExecutionAsync(CommandContext ctx)
        {
            if (Bot.IsShutdown)
                throw new ShutdownBotException();

            return Task.CompletedTask;
        }
    }
}