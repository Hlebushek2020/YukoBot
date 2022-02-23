using DSharpPlus.CommandsNext;
using System.Collections.Generic;
using YukoBot.Commands.Models;

namespace YukoBot.Commands
{
    public class CommandModule : BaseCommandModule
    {
        private static readonly Dictionary<string, Category> _categoryies = new Dictionary<string, Category>();

        public static IReadOnlyCollection<Category> Categories { get => _categoryies.Values; }

        public Category Category { get; }
        public string CommandAccessError { get; protected set; }

        public CommandModule(Category category)
        {
            // register category
            if (!_categoryies.ContainsKey(category.HelpCommand))
                _categoryies.Add(category.HelpCommand, category);

            Category = category;
        }

        public static bool CheckHelpCategoryCommand(string helpCommand) => _categoryies.ContainsKey(helpCommand);
        public static Category GetCategoryByHelpCommand(string helpCommand) => _categoryies[helpCommand];
    }
}