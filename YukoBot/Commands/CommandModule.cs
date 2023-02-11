using DSharpPlus.CommandsNext;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using YukoBot.Commands.Models;
using YukoBot.Interfaces;
using YukoBot.Models.Log;
using YukoBot.Models.Log.Providers;
using YukoBot.Settings;

namespace YukoBot.Commands
{
    public class CommandModule : BaseCommandModule
    {
        protected static readonly ILogger _defaultLogger =
            YukoLoggerFactory.Current.CreateLogger<DefaultLoggerProvider>();

        private static readonly Dictionary<string, Category> _categoryies = new Dictionary<string, Category>();

        protected static IReadOnlyYukoSettings Settings { get; } = YukoSettings.Current;

        public Category Category { get; }
        public virtual string CommandAccessError { get; }

        public CommandModule(Category category)
        {
            // register category
            if (!_categoryies.ContainsKey(category.HelpCommand))
                _categoryies.Add(category.HelpCommand, category);

            Category = category;
        }

        public static bool CheckHelpCategoryCommand(string helpCommand) => _categoryies.ContainsKey(helpCommand);
        public static Category GetCategoryByHelpCommand(string helpCommand) => _categoryies[helpCommand];
        public static IReadOnlyCollection<Category> GetCategories() => _categoryies.Values;
    }
}