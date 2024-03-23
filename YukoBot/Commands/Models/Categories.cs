namespace YukoBot.Commands.Models
{
    internal static class Categories
    {
        public static Category User { get; } = new Category(Resources.CommandCategory_User_Title, "users");

        public static Category Management { get; } = new Category(
            Resources.CommandCategory_Management_Title,
            "management",
            Resources.CommandCategory_Management_AccessError);

        public static Category CollectionManagement { get; } = new Category(
            Resources.CommandCategory_CollectionManagement_Title,
            "collection-management",
            Resources.CommandCategory_CollectionManagement_AccessError);
    }
}