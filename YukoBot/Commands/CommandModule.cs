using DSharpPlus.CommandsNext;
using System.Collections.Generic;
using YukoBot.Commands.Models;

namespace YukoBot.Commands
{
    public class CommandModule : BaseCommandModule
    {
        #region Categories
        public static Category User { get; } = new Category("Пользовательские команды", "users");
        public static Category Management { get; } = new Category("Команды управления", "management", "Команды этой категории доступны админу гильдии (сервера) и владельцу бота");
        public static Category CollectionManagement { get; } = new Category("Управление коллекциями", "collection - management", "Команды этой категории доступны для зарегистрированных и не забаненых(на этом сервере) пользователей");
        #endregion

        private static readonly Dictionary<string, Category> _categoryies = new Dictionary<string, Category>();

        public static IReadOnlyCollection<Category> Categories { get => _categoryies.Values; }

        public Category Category { get; }

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