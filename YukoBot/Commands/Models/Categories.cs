namespace YukoBot.Commands.Models
{
    internal sealed class Categories
    {
        public static Category User { get; } = new Category("Пользовательские команды", "users");
        public static Category Management { get; } = new Category("Команды управления", "management", "Команды этой категории доступны админу гильдии и владельцу бота.");
        public static Category CollectionManagement { get; } = new Category("Управление коллекциями", "collection-management", "Команды этой категории доступны для зарегистрированных и не забаненых (на этом сервере) пользователей.");
    }
}
