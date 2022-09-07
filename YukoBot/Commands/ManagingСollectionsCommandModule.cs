using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YukoBot.Commands.Attributes;
using YukoBot.Commands.Exceptions;
using YukoBot.Commands.Models;
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

        private static readonly ConcurrentDictionary<ulong, RangeInfo> _clientRanges = new ConcurrentDictionary<ulong, RangeInfo>();

        private readonly int _messageLimitSleepMs = YukoSettings.Current.DiscordMessageLimitSleepMs;

        #region Command: add (Message)
        [Command("add")]
        [Description("Добавляет вложенное сообщение в указанную коллекцию. Если коллекция не указана сообщение добавляется в коллекцию по умолчанию")]
        public async Task AddToCollection(CommandContext ctx,
            [Description("Название или Id коллекции"), RemainingText] string nameOrId = DefaultCollection)
        {
            YukoDbContext dbCtx = new YukoDbContext();
            try
            {
                DiscordMessage message = ctx.Message.ReferencedMessage;
                if (message == null)
                {
                    throw new IncorrectCommandDataException("Нет вложенного сообщения!");
                }
                DiscordEmbedBuilder discordEmbed;
                if (DefaultCollection.Equals(nameOrId, StringComparison.OrdinalIgnoreCase))
                {
                    discordEmbed = await AddToCollection(ctx, dbCtx, message);
                }
                else
                {
                    discordEmbed = await AddToCollection(ctx, dbCtx, message, nameOrId);
                }
                await ctx.Message.DeleteAsync();
                await SendSpecialMessage(ctx, discordEmbed, dbCtx);
            }
            catch (IncorrectCommandDataException ex)
            {
                await ctx.Message.DeleteAsync();
                await SendSpecialMessage(ctx, ex.ToDiscordEmbed(), dbCtx);
            }
        }
        #endregion

        #region Command: add-by-id (Message)
        [Command("add-by-id")]
        [Description("Добавляет сообщение в указанную коллекцию. Если коллекция не указана сообщение добавляется в коллекцию по умолчанию")]
        public async Task AddToCollectionById(CommandContext ctx,
            [Description("Id сообщения")] ulong messageId,
            [Description("Название или Id коллекции"), RemainingText] string nameOrId = DefaultCollection)
        {
            try
            {
                YukoDbContext dbCtx = new YukoDbContext();
                DbGuildSettings guildSettings = dbCtx.GuildsSettings.Find(ctx.Guild.Id);
                DiscordChannel discordChannel = ctx.Channel;
                bool useDefaultCollection = DefaultCollection.Equals(nameOrId, StringComparison.OrdinalIgnoreCase);
                if (guildSettings != null && guildSettings.ArtChannelId.HasValue && discordChannel.Id != guildSettings.ArtChannelId)
                {
                    discordChannel = await ctx.Client.GetChannelAsync(guildSettings.ArtChannelId.Value);
                }
                DiscordMessage message = await discordChannel.GetMessageAsync(messageId);
                DiscordEmbedBuilder discordEmbed;
                if (useDefaultCollection)
                {
                    discordEmbed = await AddToCollection(ctx, dbCtx, message);
                }
                else
                {
                    discordEmbed = await AddToCollection(ctx, dbCtx, message, nameOrId);
                }
                await ctx.RespondAsync(discordEmbed);
            }
            catch (IncorrectCommandDataException ex)
            {
                await ctx.RespondAsync(ex.ToDiscordEmbed(ctx.Member.DisplayName));
            }
        }
        #endregion

        #region Command add-range (Message)
        [Command("start")]
        [Description("Задает начальное сообщение (входит промежуток)")]
        public async Task Start(CommandContext ctx)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
               .WithTitle(ctx.Member.DisplayName)
               .WithColor(DiscordColor.Red);

            DiscordMessage message = ctx.Message.ReferencedMessage;
            if (message != null)
            {
                RangeInfo rangeInfo = new RangeInfo(message, ctx.Channel);
                if (_clientRanges.AddOrUpdate(ctx.Member.Id, rangeInfo, (k, v) => rangeInfo) != null)
                {
                    discordEmbed.WithDescription("Сохранено! (≧◡≦)").WithColor(DiscordColor.Orange);
                }
                else
                {
                    discordEmbed.WithDescription("Не удалось запомнить сообщение!");
                }
            }
            else
            {
                discordEmbed.WithDescription("Нет вложенного сообщения!");
            }
            await SendSpecialMessage(ctx, discordEmbed, new YukoDbContext());
        }

        [Command("end")]
        [Description("Задает конечное сообщение (входит в промежуток) и добавляет промежуток в заданную коллекцию")]
        public async Task End(CommandContext ctx,
            [Description("Название или Id коллекции"), RemainingText] string nameOrId = DefaultCollection)
        {
            YukoDbContext dbCtx = new YukoDbContext();
            try
            {
                ulong memberId = ctx.Member.Id;
                if (!_clientRanges.ContainsKey(memberId))
                {
                    throw new IncorrectCommandDataException("Выберите начальное изображение!");
                }
                DiscordMessage message = ctx.Message.ReferencedMessage;
                if (message == null)
                {
                    throw new IncorrectCommandDataException("Нет вложенного сообщения!");
                }
                RangeInfo rangeInfo = _clientRanges[memberId];
                DiscordChannel channel = ctx.Channel;
                if (channel.Id != rangeInfo.Channel.Id)
                {
                    throw new IncorrectCommandDataException("Начальное и конечное сообщение из разных каналов!");
                }
                bool useDefaultCollection = DefaultCollection.Equals(nameOrId, StringComparison.OrdinalIgnoreCase);
                DbCollection dbCollection;
                if (!useDefaultCollection && ulong.TryParse(nameOrId, out ulong id))
                {
                    dbCollection = dbCtx.Collections.Find(id);
                }
                else
                {
                    dbCollection = dbCtx.Collections.Where(x => x.Name == nameOrId && x.UserId == memberId).FirstOrDefault();
                    if (useDefaultCollection && dbCollection == null)
                    {
                        dbCollection = new DbCollection
                        {
                            UserId = memberId,
                            Name = DefaultCollection
                        };
                        dbCtx.Collections.Add(dbCollection);
                        await dbCtx.SaveChangesAsync();
                    }
                }
                if (dbCollection == null)
                {
                    throw new IncorrectCommandDataException("Такой коллекции нет!");
                }
                DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                    .WithTitle(ctx.Member.DisplayName)
                    .WithColor(DiscordColor.Orange)
                    .WithDescription("Операция выполняется! (≧◡≦)");
                DiscordDmChannel dmChannel = await ctx.Member.CreateDmChannelAsync();
                DiscordMessage dmMessage = await dmChannel.SendMessageAsync(discordEmbed);
                HashSet<ulong> collectionItems = dbCtx.CollectionItems
                    .Where(x => x.CollectionId == dbCollection.Id).Select(x => x.MessageId).ToHashSet();
                int limit = 10;
                bool isCompleted = false;
                ulong messageEndId = message.Id;
                ulong messageStartId = rangeInfo.StartMessage.Id;
                IReadOnlyList<DiscordMessage> messages = rangeInfo.StartMessage.ToList();
                while (!isCompleted)
                {
                    for (int numMessage = messages.Count - 1; numMessage >= 0; numMessage--)
                    {
                        message = messages[numMessage];
                        if (message.HasImages() && !collectionItems.Contains(message.Id))
                        {
                            dbCtx.CollectionItems.Add(new DbCollectionItem
                            {
                                ChannelId = channel.Id,
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
                    if (!isCompleted)
                    {
                        messages = await channel.GetMessagesAfterAsync(messageStartId, limit);
                        isCompleted = messages.Count < limit;
                        Thread.Sleep(_messageLimitSleepMs / 20);
                        if (!isCompleted)
                        {
                            messageStartId = messages.First().Id;
                        }
                    }
                }
                await dbCtx.SaveChangesAsync();
                discordEmbed.WithDescription("Сообщения успешно добавлены! (≧◡≦)");
                await dmMessage.ModifyAsync(Optional.FromValue<DiscordEmbed>(discordEmbed));

            }
            catch (IncorrectCommandDataException ex)
            {
                DbGuildSettings guildSettings = dbCtx.GuildsSettings.Find(ctx.Guild.Id);
                if (guildSettings != null && !guildSettings.AddCommandResponse)
                {
                    DiscordDmChannel dmChannel = await ctx.Member.CreateDmChannelAsync();
                    await dmChannel.SendMessageAsync(ex.ToDiscordEmbed(ctx.User.Username));
                }
                else
                {
                    await ctx.RespondAsync(ex.ToDiscordEmbed(ctx.Member.DisplayName));
                }
            }
        }
        #endregion

        #region Command: add (Collection)
        [Command("add-collection")]
        [Description("Создает новую коллекцию")]
        public async Task AddCollection(CommandContext ctx,
            [Description("Название коллекции"), RemainingText] string collectionName)
        {
            try
            {
                if (string.IsNullOrEmpty(collectionName))
                {
                    throw new IncorrectCommandDataException("Название коллекции не может быть пустым!");
                }
                YukoDbContext dbCtx = new YukoDbContext();
                DbCollection collection = dbCtx.Collections.Where(x => x.UserId == ctx.Member.Id && x.Name.Equals(collectionName)).FirstOrDefault();
                if (collection != null)
                {
                    throw new IncorrectCommandDataException($"Такая коллекция уже существует! (Id: {collection.Id})");
                }
                collection = new DbCollection
                {
                    Name = collectionName,
                    UserId = ctx.Member.Id
                };
                dbCtx.Collections.Add(collection);
                await dbCtx.SaveChangesAsync();
                DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                    .WithTitle(ctx.Member.DisplayName)
                    .WithColor(DiscordColor.Orange)
                    .WithDescription($"Коллекция создана! (Id: {collection.Id}) (≧◡≦)");
                await ctx.RespondAsync(discordEmbed);
            }
            catch (IncorrectCommandDataException ex)
            {
                await ctx.RespondAsync(ex.ToDiscordEmbed(ctx.Member.DisplayName));
            }
        }
        #endregion

        #region Command: rename-collection (Collection)
        [Command("rename-collection")]
        [Description("Переименовывает указанную коллекцию")]
        public async Task RenameCollection(CommandContext ctx,
            [Description("Id коллекции")] ulong collectionId,
            [Description("Новое название коллекции"), RemainingText] string newName)
        {
            try
            {
                YukoDbContext dbCtx = new YukoDbContext();
                DbCollection dbCollection = dbCtx.Collections.Find(collectionId);
                await RenameCollection(ctx, dbCtx, dbCollection, newName);
            }
            catch (IncorrectCommandDataException ex)
            {
                await ctx.RespondAsync(ex.ToDiscordEmbed(ctx.Member.DisplayName));
            }
        }

        [Command("rename-collection")]
        [Description("Переименовывает указанную коллекцию")]
        public async Task RenameCollection(CommandContext ctx,
            [Description("Старое название коллекции (если название коллекции содержит пробелы заключите его в кавычки: \")")] string oldName,
            [Description("Новое название коллекции (если название коллекции содержит пробелы заключите его в кавычки: \")")] string newName)
        {
            try
            {
                YukoDbContext dbCtx = new YukoDbContext();
                DbCollection dbCollection = dbCtx.Collections.Where(x => x.Name.Equals(oldName) && x.UserId == ctx.Member.Id).FirstOrDefault();
                await RenameCollection(ctx, dbCtx, dbCollection, newName);
            }
            catch (IncorrectCommandDataException ex)
            {
                await ctx.RespondAsync(ex.ToDiscordEmbed(ctx.Member.DisplayName));
            }
        }
        #endregion

        #region Command: remove-collection/item
        [Command("remove-collection")]
        [Aliases("rm-collection")]
        [Description("Удаляет коллекцию")]
        public async Task DeleteFromCollection(CommandContext ctx,
            [Description("Название или Id коллекции"), RemainingText] string nameOrId)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(ctx.Member.DisplayName);
            YukoDbContext dbContext = new YukoDbContext();
            ulong memberId = ctx.Member.Id;
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
                    .WithDescription($"Коллекция \"{dbCollection.Name}\" удалена! (≧◡≦)");
            }
            else
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Такой коллекции нет!");
            }
            await ctx.RespondAsync(discordEmbed);
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
                    .WithDescription($"Сообщение {messageId} из коллекции \"{dbCollection.Name}\" удалено! (≧◡≦)");
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
        public async Task ClearCollection(CommandContext ctx,
            [Description("Название или Id коллекции"), RemainingText] string nameOrId)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(ctx.Member.DisplayName);
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
                    collection = dbContext.Collections.Where(x => x.UserId == ctx.Member.Id && x.Name.Equals(nameOrId)).FirstOrDefault();
                }
                if (collection != null && collection.UserId == ctx.Member.Id)
                {
                    IQueryable<DbCollectionItem> items = dbContext.CollectionItems.Where(x => x.CollectionId == collection.Id);
                    dbContext.CollectionItems.RemoveRange(items);
                    await dbContext.SaveChangesAsync();
                    discordEmbed
                        .WithColor(DiscordColor.Orange)
                        .WithDescription($"Коллекция \"{collection.Name}\" очищена! (≧◡≦)");
                }
                else
                {
                    discordEmbed
                        .WithColor(DiscordColor.Red)
                        .WithDescription("Такой коллекции нет!");
                }
            }
            await ctx.RespondAsync(discordEmbed);
        }
        #endregion

        #region Command: show-collections/items
        [Command("show-collections")]
        [Aliases("collections")]
        [Description("Показывает список коллекций")]
        public async Task ShowCollections(CommandContext ctx)
        {
            YukoDbContext dbContext = new YukoDbContext();
            IQueryable<DbCollection> collections = dbContext.Collections.Where(x => x.UserId == ctx.Member.Id);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (DbCollection collection in collections)
            {
                stringBuilder.AppendLine($"{collection.Id}. {collection.Name}");
            }
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(ctx.Member.DisplayName)
                .WithColor(DiscordColor.Orange)
                .WithDescription(stringBuilder.ToString());
            await ctx.RespondAsync(discordEmbed);
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
                    IEnumerable<DbCollectionItem> items = dbContext.CollectionItems.Where(x => x.CollectionId == collection.Id).AsEnumerable().TakeLast(25);
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
        private async Task<DiscordEmbedBuilder> AddToCollection(CommandContext ctx, YukoDbContext dbCtx, DiscordMessage message)
        {
            if (!message.HasImages())
            {
                throw new IncorrectCommandDataException("Нельзя добавлять сообщения в коллекцию, если нет вложений!");
            }
            ulong memberId = ctx.Member.Id;
            DbCollection dbCollection = dbCtx.Collections.Where(x => x.UserId == memberId && x.Name == DefaultCollection).FirstOrDefault();
            if (dbCollection == null)
            {
                dbCollection = new DbCollection
                {
                    UserId = memberId,
                    Name = DefaultCollection
                };
                dbCtx.Collections.Add(dbCollection);
                await dbCtx.SaveChangesAsync();
            }
            DbCollectionItem dbCollectionItem = dbCtx.CollectionItems.Where(x => x.CollectionId == dbCollection.Id && x.MessageId == message.Id).FirstOrDefault();
            if (dbCollectionItem != null)
            {
                throw new IncorrectCommandDataException("Уже добавлено!");
            }
            dbCollectionItem = new DbCollectionItem
            {
                ChannelId = message.ChannelId,
                MessageId = message.Id,
                CollectionId = dbCollection.Id
            };
            dbCtx.CollectionItems.Add(dbCollectionItem);
            await dbCtx.SaveChangesAsync();
            return new DiscordEmbedBuilder()
                .WithTitle(ctx.Member.DisplayName)
                .WithColor(DiscordColor.Orange)
                .WithDescription("Добавлено! (≧◡≦)");
        }

        private async Task<DiscordEmbedBuilder> AddToCollection(CommandContext ctx, YukoDbContext dbCtx, DiscordMessage message, string nameOrId)
        {
            if (!message.HasImages())
            {
                throw new IncorrectCommandDataException("Нельзя добавлять сообщения в коллекцию, если нет вложений!");
            }
            ulong memberId = ctx.Member.Id;
            DbCollection dbCollection;
            if (ulong.TryParse(nameOrId, out ulong id))
            {
                dbCollection = dbCtx.Collections.Find(id);
            }
            else
            {
                dbCollection = dbCtx.Collections.Where(x => x.Name == nameOrId && x.UserId == memberId).FirstOrDefault();
            }
            if (dbCollection == null)
            {
                throw new IncorrectCommandDataException("Такой коллекции нет!");
            }
            DbCollectionItem dbCollectionItem = dbCtx.CollectionItems.Where(x => x.CollectionId == dbCollection.Id && x.MessageId == message.Id).FirstOrDefault();
            if (dbCollectionItem != null)
            {
                throw new IncorrectCommandDataException("Уже добавлено!");
            }
            dbCollectionItem = new DbCollectionItem
            {
                ChannelId = message.ChannelId,
                MessageId = message.Id,
                CollectionId = dbCollection.Id
            };
            dbCtx.CollectionItems.Add(dbCollectionItem);
            await dbCtx.SaveChangesAsync();
            return new DiscordEmbedBuilder()
                .WithTitle(ctx.Member.DisplayName)
                .WithColor(DiscordColor.Orange)
                .WithDescription("Добавлено! (≧◡≦)");
        }

        private async Task RenameCollection(CommandContext ctx, YukoDbContext dbCtx, DbCollection dbCollection, string newName)
        {
            if (dbCollection == null)
            {
                throw new IncorrectCommandDataException("Переименовываемая коллекция несуществует!");
            }
            ulong memberId = ctx.Member.Id;
            if (dbCtx.Collections.Where(x => x.Name.Equals(newName) && x.UserId == memberId).FirstOrDefault() != null)
            {
                throw new IncorrectCommandDataException($"Коллекция с названием \"{newName}\" уже существует!");
            }
            dbCollection.Name = newName;
            await dbCtx.SaveChangesAsync();
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(ctx.Member.DisplayName)
                .WithColor(DiscordColor.Orange)
                .WithDescription($"Готово! (≧◡≦)");
            await ctx.RespondAsync(discordEmbed);
        }

        private async Task SendSpecialMessage(CommandContext ctx, DiscordEmbedBuilder embed, YukoDbContext dbCtx)
        {
            DbGuildSettings guildSettings = dbCtx.GuildsSettings.Find(ctx.Guild.Id);
            if (guildSettings != null && !guildSettings.AddCommandResponse)
            {
                bool send = embed.Color.Value.Value == DiscordColor.Red.Value;
                if (!send)
                {
                    DbUser dbUser = dbCtx.Users.Find(ctx.Member.Id);
                    send = dbUser.InfoMessages;
                }
                if (send)
                {
                    DiscordDmChannel dmChannel = await ctx.Member.CreateDmChannelAsync();
                    embed.WithTitle(ctx.User.Username);
                    await dmChannel.SendMessageAsync(embed);
                }
            }
            else
            {
                embed.WithTitle(ctx.Member.DisplayName);
                await ctx.RespondAsync(embed);
            }
        }
        #endregion
    }
}