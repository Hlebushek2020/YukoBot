using System;

namespace YukoBot.Commands.Models
{
    public class Category : IEquatable<Category>
    {
        public string Name { get; }
        public string HelpCommand { get; }
        public string AccessError { get; }

        public Category(string categoryName, string helpCommand, string accessError = null)
        {
            Name = categoryName;
            HelpCommand = helpCommand;
            AccessError = accessError;
        }

        public bool Equals(Category other) => other != null && HelpCommand.Equals(other.HelpCommand);
        public override bool Equals(object obj) => Equals(obj as Category);
        public override int GetHashCode() => HelpCommand.GetHashCode();
    }
}