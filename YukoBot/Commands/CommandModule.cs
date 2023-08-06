using System.Collections.Generic;
using DSharpPlus.CommandsNext;
using YukoBot.Commands.Models;

namespace YukoBot.Commands
{
    public class CommandModule : BaseCommandModule
    {
        private static readonly Dictionary<string, Category> _categoryies = new Dictionary<string, Category>();

        public Category Category { get; }
        public string CommandAccessError { get; }

        protected CommandModule(Category category, string commandAccessError)
        {
            // register category
            if (!_categoryies.ContainsKey(category.HelpCommand))
                _categoryies.Add(category.HelpCommand, category);

            Category = category;
            CommandAccessError = commandAccessError;
        }

        protected static bool CheckHelpCategoryCommand(string helpCommand) => _categoryies.ContainsKey(helpCommand);
        protected static Category GetCategoryByHelpCommand(string helpCommand) => _categoryies[helpCommand];
        protected static IReadOnlyCollection<Category> GetCategories() => _categoryies.Values;
    }
}