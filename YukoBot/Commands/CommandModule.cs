using System.Collections.Generic;
using DSharpPlus.CommandsNext;
using YukoBot.Commands.Models;

namespace YukoBot.Commands
{
    public class CommandModule : BaseCommandModule
    {
        private static readonly Dictionary<string, Category> _registeredСategories = new Dictionary<string, Category>();

        public Category Category { get; }
        public string CommandAccessError { get; }

        protected CommandModule(Category category, string commandAccessError)
        {
            // register category
            _registeredСategories.TryAdd(category.HelpCommand, category);

            Category = category;
            CommandAccessError = commandAccessError;
        }

        protected static bool TryGetRegisteredСategory(string helpCommand, out Category category) =>
            _registeredСategories.TryGetValue(helpCommand, out category);

        protected static IReadOnlyCollection<Category> GetCategories() => _registeredСategories.Values;
    }
}