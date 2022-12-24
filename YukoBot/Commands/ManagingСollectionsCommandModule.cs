using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
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
using YukoBot.Models.Log;
using YukoBot.Models.Log.Providers;

namespace YukoBot.Commands
{
    [RequireRegisteredAndNoBan]
    public class ManagingСollectionsCommandModule : CommandModule
    {
        private const string DefaultCollection = "Default";

        private static readonly ConcurrentDictionary<ulong, RangeStartInfo> _clientRanges =
            new ConcurrentDictionary<ulong, RangeStartInfo>();

        public override string CommandAccessError =>
            "Простите, эта команда доступна для зарегистрированных и не забаненых (на этом сервере) пользователей!";

        private readonly ILogger _defaultLogger = YukoLoggerFactory.Current.CreateLogger<DefaultLoggerProvider>();

        public ManagingСollectionsCommandModule() : base(Categories.CollectionManagement)
        {
        }

        #region Command: add (Message)
        [Command("add")]
        [Description(
            "Добавить вложенное сообщение в указанную коллекцию. Если коллекция не указана сообщение добавляется в коллекцию по умолчанию.")]
        public async Task AddToCollection(CommandContext ctx,
            [Description("Название или Id коллекции (необязательно)"), RemainingText]
            string nameOrId = DefaultCollection)
        {
            YukoDbContext dbCtx = new YukoDbContext();
            try
            {
                await ctx.Message.DeleteAsync();

                DiscordMessage message = ctx.Message.ReferencedMessage;
                if (message == null)
                {
                    throw new IncorrectCommandDataException("Нет вложенного сообщения!");
                }

                DiscordEmbedBuilder discordEmbed =
                    DefaultCollection.Equals(nameOrId, StringComparison.OrdinalIgnoreCase)
                        ? await AddToCollection(ctx, dbCtx, message)
                        : await AddToCollection(ctx, dbCtx, message, nameOrId);
                await SendSpecialMessage(ctx, discordEmbed, dbCtx);
            }
            catch (IncorrectCommandDataException ex)
            {
                await SendSpecialMessage(ctx, ex.ToDiscordEmbed(), dbCtx);
            }
        }
        #endregion

        #region Command: add-by-id (Message)
        [Command("add-by-id")]
        [Description(
            "Добавить указанное сообщение в указанную коллекцию. Если коллекция не указана сообщение добавляется в коллекцию по умолчанию.")]
        public async Task AddToCollectionById(CommandContext ctx,
            [Description("Id сообщения")]
            ulong messageId,
            [Description("Название или Id коллекции (необязательно)"), RemainingText]
            string nameOrId = DefaultCollection)
        {
            try
            {
                YukoDbContext dbCtx = new YukoDbContext();
                DbGuildSettings guildSettings = dbCtx.GuildsSettings.Find(ctx.Guild.Id);
                DiscordChannel discordChannel = ctx.Channel;
                bool useDefaultCollection = DefaultCollection.Equals(nameOrId, StringComparison.OrdinalIgnoreCase);
                bool artChannel = guildSettings != null && guildSettings.ArtChannelId.HasValue &&
                                  discordChannel.Id != guildSettings.ArtChannelId;
                if (artChannel)
                {
                    try
                    {
                        discordChannel = await ctx.Client.GetChannelAsync(guildSettings.ArtChannelId.Value);
                    }
                    catch (NotFoundException)
                    {
                        throw new IncorrectCommandDataException(
                            "Канал для поиска сообщений по умолчанию не найден, обратитесь к администратору сервера!");
                    }
                }
                DiscordMessage message;
                try
                {
                    message = await discordChannel.GetMessageAsync(messageId);
                }
                catch (NotFoundException)
                {
                    if (artChannel)
                    {
                        throw new IncorrectCommandDataException(
                            "Заданное сообщение не найдено в канале для поиска сообщений");
                    }
                    else if (guildSettings == null || !guildSettings.ArtChannelId.HasValue)
                    {
                        throw new IncorrectCommandDataException(
                            "Заданное сообщение не найдено в текущем канале! Канал для поиска сообщений по умолчанию не установлен, обратитесь к администратору сервера!");
                    }
                    else
                    {
                        throw new IncorrectCommandDataException("Заданное сообщение не найдено в текущем канале!");
                    }
                }
                catch (UnauthorizedException)
                {
                    throw new IncorrectCommandDataException(
                        "Нет прав на чтение сообщений в канале для поиска сообщений");
                }

                DiscordEmbedBuilder discordEmbed = useDefaultCollection
                    ? await AddToCollection(ctx, dbCtx, message)
                    : await AddToCollection(ctx, dbCtx, message, nameOrId);
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
        [Description("Задать вложенное сообщение начальным сообщением для промежутка (входит в промежуток).")]
        public async Task Start(CommandContext ctx)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithSadTitle(ctx.Member.DisplayName)
                .WithColor(Constants.ErrorColor);
            DiscordMessage message = ctx.Message.ReferencedMessage;
            if (message != null)
            {
                RangeStartInfo rangeStartInfo = new RangeStartInfo(message, ctx.Channel);
                if (_clientRanges.AddOrUpdate(ctx.Member.Id, rangeStartInfo, (k, v) => rangeStartInfo) != null)
                {
                    discordEmbed.WithHappyMessage(ctx.Member.DisplayName, "Начальное сообщение заданно!");
                }
                else
                {
                    discordEmbed.WithDescription("Не удалось задать начальное сообщение!");
                }
            }
            else
            {
                discordEmbed.WithDescription("Нет вложенного сообщения!");
            }
            await ctx.Message.DeleteAsync();
            await SendSpecialMessage(ctx, discordEmbed, new YukoDbContext());
        }

        [Command("end")]
        [Description(
            "Задать вложенное сообщение конечным сообщением для промежутка (входит в промежуток) и добавить входящие в промежуток сообщения в заданную коллекцию. Если коллекция не указана сообщения добавляются в коллекцию по умолчанию.")]
        public async Task End(CommandContext ctx,
            [Description("Название или Id коллекции (необязательно)"), RemainingText]
            string nameOrId = DefaultCollection)
        {
            YukoDbContext dbCtx = new YukoDbContext();
            try
            {
                await ctx.Message.DeleteAsync();

                ulong memberId = ctx.Member.Id;
                if (!_clientRanges.ContainsKey(memberId))
                {
                    throw new IncorrectCommandDataException("Задайте начальное сообщение промежутка!");
                }

                if (ctx.Message.ReferencedMessage == null)
                {
                    throw new IncorrectCommandDataException("Нет вложенного сообщения!");
                }

                RangeStartInfo rangeStartInfo = _clientRanges[memberId];
                DiscordChannel channel = ctx.Channel;
                if (channel.Id != rangeStartInfo.Channel.Id)
                {
                    throw new IncorrectCommandDataException("Начальное и конечное сообщение из разных каналов!");
                }

                DbCollection dbCollection = ulong.TryParse(nameOrId, out ulong id)
                    ? dbCtx.Collections.Find(id)
                    : dbCtx.Collections.FirstOrDefault(x => x.Name == nameOrId && x.UserId == memberId);
                if (DefaultCollection.Equals(nameOrId, StringComparison.OrdinalIgnoreCase) && dbCollection == null)
                {
                    dbCollection = new DbCollection
                    {
                        UserId = memberId,
                        Name = DefaultCollection
                    };
                    dbCtx.Collections.Add(dbCollection);
                    await dbCtx.SaveChangesAsync();
                }
                if (dbCollection == null)
                {
                    throw new IncorrectCommandDataException("Такой коллекции нет!");
                }

                DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                    .WithHappyMessage(ctx.Member.DisplayName,
                        "Операция выполняется, после завершения операции это сообщение будет изменено!");
                DiscordDmChannel dmChannel = await ctx.Member.CreateDmChannelAsync();
                DiscordMessage dmMessage = await dmChannel.SendMessageAsync(discordEmbed);
                discordEmbed.WithDescription("Сообщения успешно добавлены!");

                HashSet<ulong> collectionItems = dbCtx.CollectionItems
                    .Where(x => x.CollectionId == dbCollection.Id).Select(x => x.MessageId).ToHashSet();
                DbUser dbUser = dbCtx.Users.Find(memberId);
                const int limit = 10;
                bool isCompleted = false;
                DiscordMessage discordMessage = ctx.Message.ReferencedMessage;
                ulong messageEndId = discordMessage.Id;
                ulong messageStartId = rangeStartInfo.StartMessage.Id;
                IReadOnlyList<DiscordMessage> messages = rangeStartInfo.StartMessage.ToList();
                while (!isCompleted)
                {
                    for (int numMessage = messages.Count - 1; numMessage >= 0; numMessage--)
                    {
                        discordMessage = messages[numMessage];
                        if (discordMessage.HasImages() && !collectionItems.Contains(discordMessage.Id))
                        {
                            dbCtx.CollectionItems.Add(new DbCollectionItem
                            {
                                ChannelId = channel.Id,
                                CollectionId = dbCollection.Id,
                                MessageId = discordMessage.Id
                            });
                            if (dbUser.HasPremium)
                            {
                                dbCtx.MessageLinks.Add(new DbMessage
                                {
                                    Id = discordMessage.Id,
                                    Link = string.Join(";", discordMessage.GetImages())
                                });
                            }
                            try
                            {
                                await dbCtx.SaveChangesAsync();
                            }
                            catch (Exception ex)
                            {
                                _defaultLogger.LogError(new EventId(0, "Command: end"), ex,
                                    $"{ctx.Guild.Name}, {ctx.Channel}, {discordMessage.Id}");

                                discordEmbed.WithSadTitle(ctx.Member.DisplayName).WithDescription(
                                    "Во время добавления сообщения, произошла ошибка. Добавлены не все сообщения!");
                            }
                        }
                        if (discordMessage.Id == messageEndId)
                        {
                            numMessage = -1;
                            isCompleted = true;
                        }
                    }
                    if (!isCompleted)
                    {
                        messages = await channel.GetMessagesAfterAsync(messageStartId, limit);
                        isCompleted = messages.Count < limit;
                        Thread.Sleep(Settings.DiscordMessageLimitSleepMs / 20);
                        if (!isCompleted)
                        {
                            messageStartId = messages.First().Id;
                        }
                    }
                }

                dmMessage = await dmMessage.ModifyAsync(Optional.FromValue<DiscordEmbed>(discordEmbed));
                await dmMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, Constants.DeleteMessageEmoji,
                    false));
            }
            catch (IncorrectCommandDataException ex)
            {
                await SendSpecialMessage(ctx, ex.ToDiscordEmbed(), dbCtx);
            }
        }
        #endregion

        #region Command: add (Collection)
        [Command("add-collection")]
        [Description("Создать новую коллекцию.")]
        public async Task AddCollection(CommandContext ctx,
            [Description("Название коллекции"), RemainingText]
            string collectionName)
        {
            try
            {
                if (string.IsNullOrEmpty(collectionName))
                {
                    throw new IncorrectCommandDataException("Название коллекции не может быть пустым!");
                }

                YukoDbContext dbCtx = new YukoDbContext();
                DbCollection collection =
                    dbCtx.Collections.FirstOrDefault(x => x.UserId == ctx.Member.Id && x.Name.Equals(collectionName));
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
                    .WithHappyMessage(ctx.Member.DisplayName, $"Коллекция создана! (Id: {collection.Id})");
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
        [Description("Переименовать указанную коллекцию.")]
        public async Task RenameCollection(CommandContext ctx,
            [Description("Id коллекции")]
            ulong collectionId,
            [Description("Новое название коллекции"), RemainingText]
            string newName)
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
        [Description("Переименовать указанную коллекцию.")]
        public async Task RenameCollection(CommandContext ctx,
            [Description(
                "Старое название коллекции (если название коллекции содержит пробелы заключите его в кавычки: \")")]
            string oldName,
            [Description(
                "Новое название коллекции (если название коллекции содержит пробелы заключите его в кавычки: \")")]
            string newName)
        {
            try
            {
                YukoDbContext dbCtx = new YukoDbContext();
                DbCollection dbCollection =
                    dbCtx.Collections.FirstOrDefault(x => x.Name.Equals(oldName) && x.UserId == ctx.Member.Id);
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
        [Description("Удалить коллекцию.")]
        public async Task DeleteCollection(CommandContext ctx,
            [Description("Название или Id коллекции"), RemainingText]
            string nameOrId)
        {
            DiscordEmbedBuilder discordEmbed = null;
            YukoDbContext dbContext = new YukoDbContext();
            ulong memberId = ctx.Member.Id;
            DbCollection dbCollection = ulong.TryParse(nameOrId, out ulong id)
                ? dbContext.Collections.Find(id)
                : dbContext.Collections.FirstOrDefault(x => x.Name == nameOrId && x.UserId == memberId);
            if (dbCollection != null && dbCollection.UserId == memberId)
            {
                dbContext.Collections.Remove(dbCollection);
                await dbContext.SaveChangesAsync();

                discordEmbed = new DiscordEmbedBuilder()
                    .WithHappyMessage(ctx.Member.DisplayName, $"Коллекция \"{dbCollection.Name}\" удалена!");
            }
            else
            {
                discordEmbed = new DiscordEmbedBuilder()
                    .WithSadMessage(ctx.Member.DisplayName, "Такой коллекции нет!");
            }

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("remove-item")]
        [Aliases("rm-item")]
        [Description("Удалить сообщение из коллекции.")]
        public async Task DeleteFromCollection(CommandContext commandContext,
            [Description("Id сообщения")]
            ulong messageId,
            [Description("Название или Id коллекции"), RemainingText]
            string nameOrId)
        {
            DiscordEmbedBuilder discordEmbed = null;
            YukoDbContext dbContext = new YukoDbContext();
            ulong memberId = commandContext.Member.Id;
            DbCollection dbCollection = ulong.TryParse(nameOrId, out ulong id)
                ? dbContext.Collections.Find(id)
                : dbContext.Collections.FirstOrDefault(x => x.Name == nameOrId && x.UserId == memberId);
            if (dbCollection != null && dbCollection.UserId == memberId)
            {
                IList<DbCollectionItem> dbCollectionItems = dbContext.CollectionItems
                    .Where(x => x.CollectionId == dbCollection.Id && x.MessageId == messageId).ToList();
                dbContext.CollectionItems.RemoveRange(dbCollectionItems);
                await dbContext.SaveChangesAsync();

                discordEmbed = new DiscordEmbedBuilder()
                    .WithHappyMessage(commandContext.Member.DisplayName,
                        dbCollectionItems.Count > 0
                            ? $"Сообщение {messageId} из коллекции \"{dbCollection.Name}\" удалено!"
                            : $"Ой, сообщения {messageId} и так нет в коллекции!");
            }
            else
            {
                discordEmbed = new DiscordEmbedBuilder()
                    .WithSadMessage(commandContext.Member.DisplayName, "Такой коллекции нет!");
            }

            await commandContext.RespondAsync(discordEmbed);
        }
        #endregion

        #region Command: clear-collection
        [Command("clear-collection")]
        [Description("Удалить все сообщения из коллекции.")]
        public async Task ClearCollection(CommandContext ctx,
            [Description("Название или Id коллекции"), RemainingText]
            string nameOrId)
        {
            DiscordEmbedBuilder discordEmbed = null;
            if (string.IsNullOrEmpty(nameOrId))
            {
                discordEmbed = new DiscordEmbedBuilder()
                    .WithSadMessage(ctx.Member.DisplayName, "Название или id коллекции не может быть пустым!");
            }
            else
            {
                YukoDbContext dbContext = new YukoDbContext();
                DbCollection collection = ulong.TryParse(nameOrId, out ulong id)
                    ? dbContext.Collections.Find(id)
                    : dbContext.Collections.FirstOrDefault(x => x.UserId == ctx.Member.Id && x.Name.Equals(nameOrId));
                if (collection != null && collection.UserId == ctx.Member.Id)
                {
                    IQueryable<DbCollectionItem> items =
                        dbContext.CollectionItems.Where(x => x.CollectionId == collection.Id);
                    dbContext.CollectionItems.RemoveRange(items);
                    await dbContext.SaveChangesAsync();

                    discordEmbed = new DiscordEmbedBuilder()
                        .WithHappyMessage(ctx.Member.DisplayName, $"Коллекция \"{collection.Name}\" очищена!");
                }
                else
                {
                    discordEmbed = new DiscordEmbedBuilder()
                        .WithSadMessage(ctx.Member.DisplayName, "Такой коллекции нет!");
                }
            }

            await ctx.RespondAsync(discordEmbed);
        }
        #endregion

        #region Command: show-collections/items
        [Command("show-collections")]
        [Aliases("collections")]
        [Description("Показать список коллекций.")]
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
                .WithHappyMessage(ctx.Member.DisplayName,
                    stringBuilder.Length > 0 ? stringBuilder.ToString() : "Ой, у тебя нет не одной коллекции!");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("show-items")]
        [Aliases("items")]
        [Description("Показать последние 25 сообщений коллекции.")]
        public async Task ShowItems(CommandContext commandContext,
            [Description("Название или Id коллекции"), RemainingText]
            string nameOrId)
        {
            DiscordEmbedBuilder discordEmbed = null;
            if (string.IsNullOrEmpty(nameOrId))
            {
                discordEmbed = new DiscordEmbedBuilder()
                    .WithSadMessage(commandContext.Member.DisplayName,
                        "Название или id коллекции не может быть пустым!");
            }
            else
            {
                YukoDbContext dbContext = new YukoDbContext();
                DbCollection collection = ulong.TryParse(nameOrId, out ulong id)
                    ? dbContext.Collections.Find(id)
                    : dbContext.Collections.FirstOrDefault(x =>
                        x.UserId == commandContext.Member.Id && x.Name.Equals(nameOrId));
                if (collection != null && collection.UserId == commandContext.Member.Id)
                {
                    IEnumerable<DbCollectionItem> items = dbContext.CollectionItems
                        .Where(x => x.CollectionId == collection.Id).TakeLast(25);
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (DbCollectionItem item in items)
                    {
                        stringBuilder.AppendLine(item.MessageId.ToString());
                    }

                    discordEmbed = new DiscordEmbedBuilder()
                        .WithHappyMessage(commandContext.Member.DisplayName,
                            stringBuilder.Length != 0 ? stringBuilder.ToString() : "Ой, эта коллекция пустая!");
                }
                else
                {
                    discordEmbed = new DiscordEmbedBuilder()
                        .WithSadMessage(commandContext.Member.DisplayName, "Такой коллекции нет!");
                }
            }

            await commandContext.RespondAsync(discordEmbed);
        }
        #endregion

        #region NOT COMMAND
        private static async Task<DiscordEmbedBuilder> AddToCollection(CommandContext ctx, YukoDbContext dbCtx,
            DiscordMessage message, string nameOrId = null)
        {
            if (!message.HasImages())
            {
                throw new IncorrectCommandDataException("Нельзя добавлять сообщения в коллекцию, если нет вложений!");
            }

            DbCollection dbCollection;
            ulong memberId = ctx.Member.Id;
            if (nameOrId != null)
            {
                if (ulong.TryParse(nameOrId, out ulong id))
                {
                    dbCollection = dbCtx.Collections.Find(id);
                }
                else
                {
                    dbCollection = dbCtx.Collections.FirstOrDefault(x => x.Name == nameOrId && x.UserId == memberId);
                }
                if (dbCollection == null)
                {
                    throw new IncorrectCommandDataException("Такой коллекции нет!");
                }
            }
            else
            {
                dbCollection = new DbCollection
                {
                    UserId = memberId,
                    Name = DefaultCollection
                };
                dbCtx.Collections.Add(dbCollection);
                await dbCtx.SaveChangesAsync();
            }

            DbCollectionItem dbCollectionItem =
                dbCtx.CollectionItems.FirstOrDefault(
                    x => x.CollectionId == dbCollection.Id && x.MessageId == message.Id);
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
            if (dbCtx.Users.Find(memberId).HasPremium)
            {
                dbCtx.MessageLinks.Add(new DbMessage
                {
                    Link = string.Join(";", message.GetImages()),
                    Id = message.Id
                });
            }
            await dbCtx.SaveChangesAsync();

            return new DiscordEmbedBuilder()
                .WithHappyMessage(ctx.Member.DisplayName, $"Добавлено в коллекцию \"{dbCollection.Name}\"!");
        }

        private static async Task RenameCollection(CommandContext ctx, YukoDbContext dbCtx, DbCollection dbCollection,
            string newName)
        {
            if (dbCollection == null)
            {
                throw new IncorrectCommandDataException("Переименовываемая коллекция несуществует!");
            }

            ulong memberId = ctx.Member.Id;
            if (dbCtx.Collections.FirstOrDefault(x => x.Name.Equals(newName) && x.UserId == memberId) != null)
            {
                throw new IncorrectCommandDataException($"Коллекция с названием \"{newName}\" уже существует!");
            }

            string message = $"Коллекция \"{dbCollection.Name}\" переименована в \"{newName}\"!";
            dbCollection.Name = newName;
            await dbCtx.SaveChangesAsync();

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(ctx.Member.DisplayName, message);
            await ctx.RespondAsync(discordEmbed);
        }

        private static async Task SendSpecialMessage(CommandContext ctx, DiscordEmbedBuilder embed, YukoDbContext dbCtx)
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
                    embed.WithSadTitle(ctx.User.Username);
                    DiscordDmChannel dmChannel = await ctx.Member.CreateDmChannelAsync();
                    DiscordMessage discordMessage = await dmChannel.SendMessageAsync(embed);
                    await discordMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client,
                        Constants.DeleteMessageEmoji, false));
                }
            }
            else
            {
                embed.WithSadTitle(ctx.Member.DisplayName);
                await ctx.RespondAsync(embed);
            }
        }
        #endregion
    }
}