using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YukoBot.Commands.Attributes;
using YukoBot.Extensions;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Commands
{
    [RequireRegisteredAndNoBan]
    public class ManagingСollectionsCommandModule : CommandModule
    {
        public ManagingСollectionsCommandModule() : base(Models.Category.CollectionManagement)
        {
            CommandAccessError = "Эта команда доступна для зарегистрированных и не забаненых (на этом сервере) пользователей!";
        }

        private const string DefaultCollection = "Default";

        private readonly int messageLimitSleepMs = YukoSettings.Current.DiscordMessageLimitSleepMs;

        #region Command: add (Message)
        [Command("add")]
        [Description("Добавляет вложенное сообщение в указанную коллекцию. Если коллекция не указана сообщение добавляется в коллекцию по умолчанию")]
        public async Task AddToCollection(CommandContext commandContext,
            [Description("Название или Id коллекции"), RemainingText] string nameOrId = DefaultCollection)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(commandContext.Member.DisplayName)
                .WithColor(DiscordColor.Red);

            DiscordMessage message = commandContext.Message.ReferencedMessage;
            YukoDbContext dbContext = new YukoDbContext();
            if (message != null)
            {
                if (DefaultCollection.Equals(nameOrId, StringComparison.OrdinalIgnoreCase))
                {
                    discordEmbed = await AddToCollection(commandContext, dbContext, message);
                }
                else
                {
                    discordEmbed = await AddToCollection(commandContext, dbContext, message, nameOrId);
                }
            }
            else
            {
                discordEmbed.WithDescription("Нет вложенного сообщения!");
            }
            DbGuildSettings guildSettings = dbContext.GuildsSettings.Find(commandContext.Guild.Id);
            await commandContext.Message.DeleteAsync();
            if (guildSettings != null && !guildSettings.AddCommandResponse)
            {
                bool send = discordEmbed.Color.Value.Value == DiscordColor.Red.Value;
                if (!send)
                {
                    DbUser dbUser = dbContext.Users.Find(commandContext.Member.Id);
                    send = dbUser.InfoMessages;
                }
                if (send)
                {
                    DiscordDmChannel dmChannel = await commandContext.Member.CreateDmChannelAsync();
                    await dmChannel.SendMessageAsync(discordEmbed);
                }
            }
            else
            {
                await commandContext.RespondAsync(discordEmbed);
            }
        }
        #endregion

        #region Command: add-by-id (Message)
        [Command("add-by-id")]
        [Description("Добавляет сообщение в указанную коллекцию. Если коллекция не указана сообщение добавляется в коллекцию по умолчанию")]
        public async Task AddToCollectionById(CommandContext commandContext,
            [Description("Id сообщения")] ulong messageId,
            [Description("Название или Id коллекции"), RemainingText] string nameOrId = DefaultCollection)
        {
            YukoDbContext dbContext = new YukoDbContext();
            DbGuildSettings guildSettings = dbContext.GuildsSettings.Find(commandContext.Guild.Id);
            DiscordChannel discordChannel = commandContext.Channel;
            bool useDefaultCollection = DefaultCollection.Equals(nameOrId, StringComparison.OrdinalIgnoreCase);
            if (guildSettings != null && guildSettings.ArtChannelId.HasValue && discordChannel.Id != guildSettings.ArtChannelId)
            {
                discordChannel = await commandContext.Client.GetChannelAsync(guildSettings.ArtChannelId.Value);
            }
            DiscordMessage message = await discordChannel.GetMessageAsync(messageId);
            DiscordEmbedBuilder discordEmbed;
            if (useDefaultCollection)
            {
                discordEmbed = await AddToCollection(commandContext, dbContext, message);
            }
            else
            {
                discordEmbed = await AddToCollection(commandContext, dbContext, message, nameOrId);
            }
            await commandContext.RespondAsync(discordEmbed);
        }
        #endregion

        #region Command: add-range (Message)
        [Command("add-range")]
        [Description("Добавляет сообщения (имеющие вложения) из заданного промежутка в указанную коллекцию. Если коллекция не указана сообщение добавляется в коллекцию по умолчанию")]
        public async Task AddRange(CommandContext commandContext,
            [Description("Id начального сообщения (не входит в промежуток)")] ulong messageStartId,
            [Description("Id конечного сообщения (входит в промежуток)")] ulong messageEndId,
            [Description("Название или Id коллекции"), RemainingText] string nameOrId = DefaultCollection)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
               .WithTitle(commandContext.Member.DisplayName);

            DiscordChannel discordChannel = commandContext.Channel;
            YukoDbContext dbContext = new YukoDbContext();
            DbGuildSettings guildSettings = dbContext.GuildsSettings.Find(commandContext.Guild.Id);
            if (guildSettings != null && guildSettings.ArtChannelId.HasValue && discordChannel.Id != guildSettings.ArtChannelId)
            {
                discordChannel = await commandContext.Client.GetChannelAsync(guildSettings.ArtChannelId.Value);
            }
            bool useDefaultCollection = DefaultCollection.Equals(nameOrId, StringComparison.OrdinalIgnoreCase);
            ulong memberId = commandContext.Member.Id;
            DbCollection dbCollection;
            if (!useDefaultCollection && ulong.TryParse(nameOrId, out ulong id))
            {
                dbCollection = dbContext.Collections.Find(id);
            }
            else
            {
                dbCollection = dbContext.Collections.Where(x => x.Name == nameOrId && x.UserId == memberId).FirstOrDefault();
                if (useDefaultCollection && dbCollection == null)
                {
                    dbCollection = new DbCollection
                    {
                        UserId = memberId,
                        Name = DefaultCollection
                    };
                    dbContext.Collections.Add(dbCollection);
                    await dbContext.SaveChangesAsync();
                }
            }
            if (dbCollection != null && dbCollection.UserId == memberId)
            {
                discordEmbed
                    .WithColor(DiscordColor.Orange)
                    .WithDescription("Уведомление о завершении операции будет отправлено в личные сообщения. ≧◡≦");
                await commandContext.RespondAsync(discordEmbed);
                HashSet<ulong> collectionItems = dbContext.CollectionItems
                   .Where(x => x.CollectionId == dbCollection.Id).Select(x => x.MessageId).ToHashSet();
                int limit = 10;
                bool isCompleted = false;
                while (!isCompleted)
                {
                    IReadOnlyList<DiscordMessage> messages = await discordChannel.GetMessagesAfterAsync(messageStartId, limit);
                    isCompleted = messages.Count < limit;
                    for (int numMessage = messages.Count - 1; numMessage >= 0; numMessage--)
                    {
                        DiscordMessage message = messages[numMessage];
                        if (message.HasImages() && !collectionItems.Contains(message.Id))
                        {
                            dbContext.CollectionItems.Add(new DbCollectionItem
                            {
                                ChannelId = discordChannel.Id,
                                CollectionId = dbCollection.Id,
                                MessageId = message.Id
                            });
                        }
                        if (message.Id == messageEndId)
                        {
                            numMessage = -1;
                            isCompleted = true;
                        }
                    }
                    Thread.Sleep(messageLimitSleepMs / 20);
                    messageStartId = messages.First().Id;
                }
                await dbContext.SaveChangesAsync();
                DiscordDmChannel dmChannel = await commandContext.Member.CreateDmChannelAsync();
                discordEmbed.WithDescription("Сообщения успешно добавлены! ≧◡≦");
                await dmChannel.SendMessageAsync(discordEmbed);
            }
            else
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Такой коллекции нет!");
                await commandContext.RespondAsync(discordEmbed);
            }
        }
        #endregion

        #region Command: add (Collection)
        [Command("add-collection")]
        [Description("Создает новую коллекцию")]
        public async Task AddCollection(CommandContext commandContext,
            [Description("Название коллекции"), RemainingText] string collectionName)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(commandContext.Member.DisplayName);
            if (string.IsNullOrEmpty(collectionName))
            {
                discordEmbed
                   .WithColor(DiscordColor.Red)
                   .WithDescription("Название коллекции не может быть пустым!");
            }
            else
            {
                YukoDbContext dbContext = new YukoDbContext();
                DbCollection collection = dbContext.Collections.Where(x => x.UserId == commandContext.Member.Id && x.Name.Equals(collectionName)).FirstOrDefault();
                if (collection == null)
                {
                    collection = new DbCollection
                    {
                        Name = collectionName,
                        UserId = commandContext.Member.Id
                    };
                    dbContext.Collections.Add(collection);
                    await dbContext.SaveChangesAsync();
                    discordEmbed
                        .WithColor(DiscordColor.Orange)
                        .WithDescription($"Коллекция создана! (Id: {collection.Id}) ≧◡≦");
                }
                else
                {
                    discordEmbed
                        .WithColor(DiscordColor.Red)
                        .WithDescription($"Такая коллекция уже существует! (Id: {collection.Id})");
                }
            }
            await commandContext.RespondAsync(discordEmbed);
        }
        #endregion

        #region Command: rename-collection (Collection)
        [Command("rename-collection")]
        [Description("Переименовывает указанную коллекцию")]
        public async Task RenameCollection(CommandContext commandContext,
            [Description("Id коллекции")] ulong collectionId,
            [Description("Новое название коллекции"), RemainingText] string newName)
        {
            YukoDbContext dbContext = new YukoDbContext();
            DbCollection dbCollection = dbContext.Collections.Find(collectionId);
            await RenameCollection(commandContext, dbContext, dbCollection, newName);
        }

        [Command("rename-collection")]
        [Description("Переименовывает указанную коллекцию")]
        public async Task RenameCollection(CommandContext commandContext,
            [Description("Старое название коллекции (если название коллекции содержит пробелы заключите его в кавычки: \")")] string oldName,
            [Description("Новое название коллекции (если название коллекции содержит пробелы заключите его в кавычки: \")")] string newName)
        {
            YukoDbContext dbContext = new YukoDbContext();
            DbCollection dbCollection = dbContext.Collections.Where(x => x.Name.Equals(oldName) && x.UserId == commandContext.Member.Id).FirstOrDefault();
            await RenameCollection(commandContext, dbContext, dbCollection, newName);
        }
        #endregion

        #region Command: remove-collection/item
        [Command("remove-collection")]
        [Aliases("rm-collection")]
        [Description("Удаляет коллекцию")]
        public async Task DeleteFromCollection(CommandContext commandContext,
            [Description("Название или Id коллекции"), RemainingText] string nameOrId)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(commandContext.Member.DisplayName);
            YukoDbContext dbContext = new YukoDbContext();
            ulong memberId = commandContext.Member.Id;
            DbCollection dbCollection;
            if (ulong.TryParse(nameOrId, out ulong id))
            {
                dbCollection = dbContext.Collections.Find(id);
            }
            else
            {
                dbCollection = dbContext.Collections.Where(x => x.Name == nameOrId && x.UserId == memberId).FirstOrDefault();
            }
            if (dbCollection != null && dbCollection.UserId == memberId)
            {
                dbContext.Collections.Remove(dbCollection);
                await dbContext.SaveChangesAsync();
                discordEmbed
                    .WithColor(DiscordColor.Orange)
                    .WithDescription($"Коллекция \"{dbCollection.Name}\" удалена! ≧◡≦");
            }
            else
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Такой коллекции нет!");
            }
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("remove-item")]
        [Aliases("rm-item")]
        [Description("Удалить сообщение из коллекции")]
        public async Task DeleteFromCollection(CommandContext commandContext,
            [Description("Id сообщения")] ulong messageId,
            [Description("Название или Id коллекции"), RemainingText] string nameOrId)
        {

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(commandContext.Member.DisplayName);
            YukoDbContext dbContext = new YukoDbContext();
            ulong memberId = commandContext.Member.Id;
            DbCollection dbCollection;
            if (ulong.TryParse(nameOrId, out ulong id))
            {
                dbCollection = dbContext.Collections.Find(id);
            }
            else
            {
                dbCollection = dbContext.Collections.Where(x => x.Name == nameOrId && x.UserId == memberId).FirstOrDefault();
            }
            if (dbCollection != null && dbCollection.UserId == memberId)
            {
                IQueryable<DbCollectionItem> dbCollectionItems = dbContext.CollectionItems.Where(x => x.CollectionId == dbCollection.Id && x.MessageId == messageId);
                dbContext.CollectionItems.RemoveRange(dbCollectionItems);
                await dbContext.SaveChangesAsync();
                discordEmbed
                    .WithColor(DiscordColor.Orange)
                    .WithDescription($"Сообщение {messageId} из коллекции \"{dbCollection.Name}\" удалено! ≧◡≦");
            }
            else
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Такой коллекции нет!");
            }
            await commandContext.RespondAsync(discordEmbed);
        }
        #endregion

        #region Command: clear-collection
        [Command("clear-collection")]
        [Description("Удаляет все сообщения из коллекции")]
        public async Task ClearCollection(CommandContext commandContext,
            [Description("Название или Id коллекции"), RemainingText] string nameOrId)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(commandContext.Member.DisplayName);
            if (string.IsNullOrEmpty(nameOrId))
            {
                discordEmbed
                   .WithColor(DiscordColor.Red)
                   .WithDescription("Название или id коллекции не может быть пустым!");
            }
            else
            {
                YukoDbContext dbContext = new YukoDbContext();
                DbCollection collection;
                if (ulong.TryParse(nameOrId, out ulong id))
                {
                    collection = dbContext.Collections.Find(id);
                }
                else
                {
                    collection = dbContext.Collections.Where(x => x.UserId == commandContext.Member.Id && x.Name.Equals(nameOrId)).FirstOrDefault();
                }
                if (collection != null && collection.UserId == commandContext.Member.Id)
                {
                    IQueryable<DbCollectionItem> items = dbContext.CollectionItems.Where(x => x.CollectionId == collection.Id);
                    dbContext.CollectionItems.RemoveRange(items);
                    await dbContext.SaveChangesAsync();
                    discordEmbed
                        .WithColor(DiscordColor.Orange)
                        .WithDescription($"Коллекция \"{collection.Name}\" очищена! ≧◡≦");
                }
                else
                {
                    discordEmbed
                        .WithColor(DiscordColor.Red)
                        .WithDescription("Такой коллекции нет!");
                }
            }
            await commandContext.RespondAsync(discordEmbed);
        }
        #endregion

        #region Command: show-collections/items
        [Command("show-collections")]
        [Aliases("collections")]
        [Description("Показывает список коллекций")]
        public async Task ShowCollections(CommandContext commandContext)
        {
            YukoDbContext dbContext = new YukoDbContext();
            IQueryable<DbCollection> collections = dbContext.Collections.Where(x => x.UserId == commandContext.Member.Id);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (DbCollection collection in collections)
            {
                stringBuilder.AppendLine($"{collection.Id}. {collection.Name}");
            }
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(commandContext.Member.DisplayName)
                .WithColor(DiscordColor.Orange)
                .WithDescription(stringBuilder.ToString());
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("show-items")]
        [Aliases("items")]
        [Description("Показывает последние 25 сообщений коллекции")]
        public async Task ShowItems(CommandContext commandContext,
            [Description("Название или Id коллекции"), RemainingText] string nameOrId)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(commandContext.Member.DisplayName);
            if (string.IsNullOrEmpty(nameOrId))
            {
                discordEmbed
                   .WithColor(DiscordColor.Red)
                   .WithDescription("Название или id коллекции не может быть пустым!");
            }
            else
            {
                YukoDbContext dbContext = new YukoDbContext();
                DbCollection collection;
                if (ulong.TryParse(nameOrId, out ulong id))
                {
                    collection = dbContext.Collections.Find(id);
                }
                else
                {
                    collection = dbContext.Collections.Where(x => x.UserId == commandContext.Member.Id && x.Name.Equals(nameOrId)).FirstOrDefault();
                }
                if (collection != null && collection.UserId == commandContext.Member.Id)
                {
                    IQueryable<DbCollectionItem> items = dbContext.CollectionItems.Where(x => x.CollectionId == collection.Id).TakeLast(25);
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (DbCollectionItem item in items)
                    {
                        stringBuilder.AppendLine(item.MessageId.ToString());
                    }
                    discordEmbed
                        .WithColor(DiscordColor.Orange)
                        .WithDescription(stringBuilder.Length != 0 ? stringBuilder.ToString() : "Ой, эта коллекция пустая!");
                }
                else
                {
                    discordEmbed
                        .WithColor(DiscordColor.Red)
                        .WithDescription("Такой коллекции нет!");
                }
            }
            await commandContext.RespondAsync(discordEmbed);
        }
        #endregion

        #region NOT COMMAND
        private async Task<DiscordEmbedBuilder> AddToCollection(CommandContext commandContext, YukoDbContext dbContext, DiscordMessage message)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(commandContext.Member.DisplayName);

            if (message.HasImages())
            {
                ulong memberId = commandContext.Member.Id;
                DbCollection dbCollection = dbContext.Collections.Where(x => x.UserId == memberId && x.Name == DefaultCollection).FirstOrDefault();
                if (dbCollection == null)
                {
                    dbCollection = new DbCollection
                    {
                        UserId = memberId,
                        Name = DefaultCollection
                    };
                    dbContext.Collections.Add(dbCollection);
                    await dbContext.SaveChangesAsync();
                }
                DbCollectionItem dbCollectionItem = dbContext.CollectionItems.Where(x => x.CollectionId == dbCollection.Id && x.MessageId == message.Id).FirstOrDefault();
                if (dbCollectionItem != null)
                {
                    discordEmbed
                        .WithColor(DiscordColor.Red)
                        .WithDescription("Уже добавлено!");
                }
                else
                {
                    dbCollectionItem = new DbCollectionItem
                    {
                        ChannelId = message.ChannelId,
                        MessageId = message.Id,
                        CollectionId = dbCollection.Id
                    };
                    dbContext.CollectionItems.Add(dbCollectionItem);
                    await dbContext.SaveChangesAsync();
                    discordEmbed
                        .WithColor(DiscordColor.Orange)
                        .WithDescription("Добавлено! ≧◡≦");
                }
            }
            else
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Нельзя добавлять сообщения в коллекцию, если нет вложений!");
            }
            return discordEmbed;
        }

        private async Task<DiscordEmbedBuilder> AddToCollection(CommandContext commandContext, YukoDbContext dbContext, DiscordMessage message, string nameOrId)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(commandContext.Member.DisplayName);

            if (message.HasImages())
            {
                ulong memberId = commandContext.Member.Id;
                DbCollection dbCollection;
                if (ulong.TryParse(nameOrId, out ulong id))
                {
                    dbCollection = dbContext.Collections.Find(id);
                }
                else
                {
                    dbCollection = dbContext.Collections.Where(x => x.Name == nameOrId && x.UserId == memberId).FirstOrDefault();
                }
                if (dbCollection != null && dbCollection.UserId == memberId)
                {
                    DbCollectionItem dbCollectionItem = dbContext.CollectionItems.Where(x => x.CollectionId == dbCollection.Id && x.MessageId == message.Id).FirstOrDefault();
                    if (dbCollectionItem != null)
                    {
                        discordEmbed
                            .WithColor(DiscordColor.Red)
                            .WithDescription("Уже добавлено!");
                    }
                    else
                    {
                        dbCollectionItem = new DbCollectionItem
                        {
                            ChannelId = message.ChannelId,
                            MessageId = message.Id,
                            CollectionId = dbCollection.Id
                        };
                        dbContext.CollectionItems.Add(dbCollectionItem);
                        await dbContext.SaveChangesAsync();
                        discordEmbed
                            .WithColor(DiscordColor.Orange)
                            .WithDescription("Добавлено! ≧◡≦");
                    }
                }
                else
                {
                    discordEmbed
                        .WithColor(DiscordColor.Red)
                        .WithDescription("Такой коллекции нет!");
                }
            }
            else
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Нельзя добавлять сообщения в коллекцию, если нет вложений!");
            }
            return discordEmbed;
        }

        private async Task RenameCollection(CommandContext commandContext, YukoDbContext dbContext, DbCollection dbCollection, string newName)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(commandContext.Member.DisplayName);
            ulong memberId = commandContext.Member.Id;
            if (dbCollection != null && dbCollection.UserId == memberId)
            {
                if (dbContext.Collections.Where(x => x.Name.Equals(newName) && x.UserId == memberId).FirstOrDefault() == null)
                {
                    dbCollection.Name = newName;
                    await dbContext.SaveChangesAsync();
                    discordEmbed
                        .WithColor(DiscordColor.Orange)
                        .WithDescription($"Готово! ≧◡≦");
                }
                else
                {
                    discordEmbed
                        .WithColor(DiscordColor.Red)
                        .WithDescription($"Коллекция с названием \"{newName}\" уже существует!");
                }
            }
            else
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Переименовываемая коллекция несуществует!");
            }
            await commandContext.RespondAsync(discordEmbed);
        }
        #endregion
    }
}