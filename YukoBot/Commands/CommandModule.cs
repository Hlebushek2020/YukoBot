﻿using DSharpPlus.CommandsNext;
using System.Collections.Generic;
using YukoBot.Commands.Models;
using YukoBot.Interfaces;

namespace YukoBot.Commands
{
    public class CommandModule : BaseCommandModule
    {
        private static readonly Dictionary<string, Category> _categoryies = new Dictionary<string, Category>();

        public static IReadOnlyYukoSettings Settings { get; } = YukoSettings.Current;

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
        public static IReadOnlyCollection<Category> GetCategories() => _categoryies.Values;
    }
}