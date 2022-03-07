namespace YukoBot.Commands.Models
{
    public class Category
    {
        #region Categories
        public static Category User { get; } = new Category("Пользовательские команды", "users");
        public static Category Management { get; } = new Category("Команды управления", "management", "Команды этой категории доступны админу гильдии (сервера) и владельцу бота");
        public static Category CollectionManagement { get; } = new Category("Управление коллекциями", "collection-management", "Команды этой категории доступны для зарегистрированных и не забаненых (на этом сервере) пользователей");
        #endregion

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
