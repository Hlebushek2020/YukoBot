namespace YukoBot.Commands.Models
{
    public class Category
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
    }
}
