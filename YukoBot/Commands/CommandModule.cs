using System.Collections.Generic;
using DSharpPlus.CommandsNext;
using Microsoft.Extensions.Logging;
using YukoBot.Commands.Models;
using YukoBot.Interfaces;
using YukoBot.Settings;

namespace YukoBot.Commands
{
    public class CommandModule : BaseCommandModule
    {
        protected static readonly ILogger _defaultLogger =
            YukoLoggerFactory.Current.CreateLogger<DefaultLoggerProvider>();

        private static readonly Dictionary<string, Category> _categoryies = new Dictionary<string, Category>();

        protected static IYukoSettings Settings { get; } = YukoSettings.Current;

        public Category Category { get; }
        public virtual string CommandAccessError { get; }

        protected CommandModule(Category category)
        {
            // register category
            if (!_categoryies.ContainsKey(category.HelpCommand))
                _categoryies.Add(category.HelpCommand, category);

            Category = category;
        }

        protected static bool CheckHelpCategoryCommand(string helpCommand) => _categoryies.ContainsKey(helpCommand);
        protected static Category GetCategoryByHelpCommand(string helpCommand) => _categoryies[helpCommand];
        protected static IReadOnlyCollection<Category> GetCategories() => _categoryies.Values;
    }
}