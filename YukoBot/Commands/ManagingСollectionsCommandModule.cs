using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
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
        private const string DefaultCollection = "Default";

        private static readonly ConcurrentDictionary<ulong, RangeStartInfo> _clientRanges =
            new ConcurrentDictionary<ulong, RangeStartInfo>();

        private readonly YukoDbContext _dbContext;
        private readonly IYukoSettings _yukoSettings;
        private readonly ILogger<ManagingСollectionsCommandModule> _logger;

        public ManagingСollectionsCommandModule(
            YukoDbContext dbContext,
            IYukoSettings yukoSettings,
            ILogger<ManagingСollectionsCommandModule> logger)
            : base(Categories.CollectionManagement, Resources.ManagingСollectionsCommand_AccessError)
        {
            _dbContext = dbContext;
            _yukoSettings = yukoSettings;
            _logger = logger;
        }

        #region Command: add (Message)
        [Command("add")]
        [Description(
            "Добавить вложенное сообщение в указанную коллекцию. Если коллекция не указана сообщение добавляется в коллекцию по умолчанию.")]
        public async Task AddToCollection(
            CommandContext ctx,
            [Description("Название или Id коллекции"), RemainingText]
            string nameOrId = DefaultCollection)
        {
            try
            {
                await ctx.Message.DeleteAsync();

                DiscordMessage message = ctx.Message.ReferencedMessage;
                if (message == null)
                    throw new IncorrectCommandDataException(
                        Resources.ManagingСollectionsCommand_AddToCollection_ReferencedMessageNotFound);

                DiscordEmbedBuilder discordEmbed = await AddToCollection(ctx, message, nameOrId);
                await SendSpecialMessage(ctx, discordEmbed);
            }
            catch (IncorrectCommandDataException ex)
            {
                await SendSpecialMessage(ctx, ex.ToDiscordEmbed(ctx.Member.DisplayName));
            }
        }
        #endregion

        #region Command: add-by-id (Message)
        [Command("add-by-id")]
        [Description(
            "Добавить указанное сообщение в указанную коллекцию. Если коллекция не указана сообщение добавляется в коллекцию по умолчанию.")]
        public async Task AddToCollectionById(
            CommandContext ctx,
            [Description("Id сообщения")]
            ulong messageId,
            [Description("Название или Id коллекции"), RemainingText]
            string nameOrId = DefaultCollection)
        {
            try
            {
                DbGuildSettings guildSettings = await _dbContext.GuildsSettings.FindAsync(ctx.Guild.Id);
                DiscordChannel discordChannel = ctx.Channel;
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
                            Resources.ManagingСollectionsCommand_AddToCollectionById_ArtChannelNotFound);
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
                        throw new IncorrectCommandDataException(
                            Resources.ManagingСollectionsCommand_AddToCollectionById_MessageNotFoundInArtChannel);

                    if (guildSettings == null || !guildSettings.ArtChannelId.HasValue)
                        throw new IncorrectCommandDataException(Resources
                            .ManagingСollectionsCommand_AddToCollectionById_MessageNotFoundAndArtChannelNotSet);

                    throw new IncorrectCommandDataException(
                        Resources.ManagingСollectionsCommand_AddToCollectionById_MessageNotFound);
                }
                catch (UnauthorizedException)
                {
                    throw new IncorrectCommandDataException(
                        artChannel
                            ? Resources.ManagingСollectionsCommand_AddToCollectionById_NoArtChannelAccess
                            : Resources.ManagingСollectionsCommand_AddToCollectionById_NoChannelAccess);
                }

                DiscordEmbedBuilder discordEmbed = await AddToCollection(ctx, message, nameOrId);
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
                    discordEmbed.WithHappyMessage(ctx.Member.DisplayName,
                        Resources.ManagingСollectionsCommand_Start_IsSet);
                }
                else
                {
                    discordEmbed.WithDescription(Resources.ManagingСollectionsCommand_Start_IsNotSet);
                }
            }
            else
            {
                discordEmbed.WithDescription(Resources.ManagingСollectionsCommand_Start_NoReferencedMessage);
            }
            await ctx.Message.DeleteAsync();
            await SendSpecialMessage(ctx, discordEmbed);
        }

        [Command("end")]
        [Description(
            "Задать вложенное сообщение конечным сообщением для промежутка (входит в промежуток) и добавить входящие в промежуток сообщения в заданную коллекцию. Если коллекция не указана сообщения добавляются в коллекцию по умолчанию.")]
        public async Task End(
            CommandContext ctx,
            [Description("Название или Id коллекции"), RemainingText]
            string nameOrId = DefaultCollection)
        {
            try
            {
                await ctx.Message.DeleteAsync();

                ulong memberId = ctx.Member.Id;
                if (!_clientRanges.ContainsKey(memberId))
                    throw new IncorrectCommandDataException(
                        Resources.ManagingСollectionsCommand_End_StartMessageIsNotSet);

                if (ctx.Message.ReferencedMessage == null)
                    throw new IncorrectCommandDataException(
                        Resources.ManagingСollectionsCommand_End_NoReferencedMessage);

                RangeStartInfo rangeStartInfo = _clientRanges[memberId];
                DiscordChannel channel = ctx.Channel;

                if (channel.Id != rangeStartInfo.Channel.Id)
                    throw new IncorrectCommandDataException(
                        Resources.ManagingСollectionsCommand_End_DifferentChannels);

                DbCollection dbCollection = await GetOrCreateCollection(memberId, nameOrId);

                DiscordEmbedBuilder dmEmbed = new DiscordEmbedBuilder()
                    .WithHappyMessage(
                        ctx.Member.DisplayName,
                        Resources.ManagingСollectionsCommand_End_StartOfExecution);
                DiscordDmChannel dmChannel = await ctx.Member.CreateDmChannelAsync();
                DiscordMessage dmMessage = await dmChannel.SendMessageAsync(dmEmbed);
                dmEmbed.WithDescription(string.Format(
                    Resources.ManagingСollectionsCommand_End_EndOfExecution,
                    dbCollection.Name));

                HashSet<ulong> collectionItems = _dbContext.CollectionItems
                    .Where(x => x.CollectionId == dbCollection.Id).Select(x => x.MessageId).ToHashSet();

                DbUser dbUser = await _dbContext.Users.FindAsync(memberId);
                bool hasPremiumAccess = dbUser.HasPremiumAccess;

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
                        if (discordMessage.HasImages(_yukoSettings) && !collectionItems.Contains(discordMessage.Id))
                        {
                            await SaveCollectionItem(
                                ctx,
                                discordMessage,
                                dmEmbed,
                                dbCollection,
                                hasPremiumAccess);
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
                        Thread.Sleep(_yukoSettings.DiscordMessageLimitSleepMs / 20);
                        if (!isCompleted)
                        {
                            messageStartId = messages.First().Id;
                        }
                    }
                }

                dmMessage = await dmMessage.ModifyAsync(Optional.FromValue<DiscordEmbed>(dmEmbed));
                await dmMessage.CreateReactionAsync(
                    DiscordEmoji.FromName(
                        ctx.Client,
                        Constants.DeleteMessageEmoji,
                        false));
            }
            catch (IncorrectCommandDataException ex)
            {
                await SendSpecialMessage(ctx, ex.ToDiscordEmbed(ctx.Member.DisplayName));
            }
        }
        #endregion

        #region Command: add (Collection)
        [Command("add-collection")]
        [Description("Создать новую коллекцию.")]
        public async Task AddCollection(
            CommandContext ctx,
            [Description("Название коллекции"), RemainingText]
            string collectionName)
        {
            try
            {
                if (string.IsNullOrEmpty(collectionName))
                    throw new IncorrectCommandDataException(
                        Resources.ManagingСollectionsCommand_AddCollection_NameIsEmpty);

                DbCollection collection =
                    _dbContext.Collections.FirstOrDefault(
                        x => x.UserId == ctx.Member.Id && x.Name.Equals(collectionName));
                if (collection != null)
                    throw new IncorrectCommandDataException(string.Format(
                        Resources.ManagingСollectionsCommand_AddCollection_CollectionExists,
                        collection.Id));

                collection = new DbCollection
                {
                    Name = collectionName,
                    UserId = ctx.Member.Id
                };
                _dbContext.Collections.Add(collection);
                await _dbContext.SaveChangesAsync();

                DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                    .WithHappyMessage(
                        ctx.Member.DisplayName,
                        string.Format(
                            Resources.ManagingСollectionsCommand_AddCollection_Created,
                            collection.Id));
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
        public async Task RenameCollection(
            CommandContext ctx,
            [Description("Id коллекции")]
            ulong collectionId,
            [Description("Новое название коллекции"), RemainingText]
            string newName)
        {
            try
            {
                DbCollection dbCollection = await _dbContext.Collections.FindAsync(collectionId);
                await RenameCollection(ctx, dbCollection, newName);
            }
            catch (IncorrectCommandDataException ex)
            {
                await ctx.RespondAsync(ex.ToDiscordEmbed(ctx.Member.DisplayName));
            }
        }

        [Command("rename-collection")]
        [Description("Переименовать указанную коллекцию.")]
        public async Task RenameCollection(
            CommandContext ctx,
            [Description(
                "Старое название коллекции (если название коллекции содержит пробелы заключите его в кавычки: \")")]
            string oldName,
            [Description(
                "Новое название коллекции (если название коллекции содержит пробелы заключите его в кавычки: \")")]
            string newName)
        {
            try
            {
                DbCollection dbCollection =
                    _dbContext.Collections.FirstOrDefault(x => x.Name.Equals(oldName) && x.UserId == ctx.Member.Id);
                await RenameCollection(ctx, dbCollection, newName);
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
        public async Task DeleteCollection(
            CommandContext ctx,
            [Description("Название или Id коллекции"), RemainingText]
            string nameOrId)
        {
            DiscordEmbedBuilder discordEmbed = null;
            ulong memberId = ctx.Member.Id;
            DbCollection dbCollection = ulong.TryParse(nameOrId, out ulong id)
                ? await _dbContext.Collections.FindAsync(id)
                : _dbContext.Collections.FirstOrDefault(x => x.Name == nameOrId && x.UserId == memberId);
            if (dbCollection != null && dbCollection.UserId == memberId)
            {
                _dbContext.Collections.Remove(dbCollection);
                await _dbContext.SaveChangesAsync();

                discordEmbed = new DiscordEmbedBuilder()
                    .WithHappyMessage(
                        ctx.Member.DisplayName,
                        string.Format(
                            Resources.ManagingСollectionsCommand_DeleteCollection_Deleted,
                            dbCollection.Name));
            }
            else
            {
                discordEmbed = new DiscordEmbedBuilder()
                    .WithSadMessage(
                        ctx.Member.DisplayName,
                        Resources.ManagingСollectionsCommand_DeleteCollection_CollectionNotFound);
            }

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("remove-item")]
        [Description("Удалить сообщение из коллекции.")]
        public async Task DeleteFromCollection(
            CommandContext ctx,
            [Description("Id сообщения")]
            ulong messageId,
            [Description("Название или Id коллекции"), RemainingText]
            string nameOrId = DefaultCollection)
        {
            try
            {
                await PrivateDeleteFromCollection(ctx, messageId, nameOrId);
            }
            catch (IncorrectCommandDataException ex)
            {
                await SendSpecialMessage(ctx, ex.ToDiscordEmbed(ctx.Member.DisplayName));
            }
        }

        [Command("remove")]
        [Aliases("rm")]
        [Description("Удалить вложенное сообщение из коллекции.")]
        public async Task DeleteFromCollection(
            CommandContext ctx,
            [Description("Название или Id коллекции"), RemainingText]
            string nameOrId = DefaultCollection)
        {
            try
            {
                await ctx.Message.DeleteAsync();

                DiscordMessage message = ctx.Message.ReferencedMessage;
                if (message == null)
                {
                    throw new IncorrectCommandDataException("Простите, нет вложенного сообщения!");
                }

                await PrivateDeleteFromCollection(ctx, message.Id, nameOrId);
            }
            catch (IncorrectCommandDataException ex)
            {
                await SendSpecialMessage(ctx, ex.ToDiscordEmbed(ctx.Member.DisplayName));
            }
        }
        #endregion

        #region Command: clear-collection
        [Command("clear-collection")]
        [Description("Удалить все сообщения из коллекции.")]
        public async Task ClearCollection(
            CommandContext ctx,
            [Description("Название или Id коллекции"), RemainingText]
            string nameOrId)
        {
            DiscordEmbedBuilder discordEmbed = null;
            if (string.IsNullOrEmpty(nameOrId))
            {
                discordEmbed = new DiscordEmbedBuilder()
                    .WithSadMessage(
                        ctx.Member.DisplayName,
                        Resources.ManagingСollectionsCommand_ClearCollection_NameOrIdIsEmpty);
            }
            else
            {
                DbCollection collection = ulong.TryParse(nameOrId, out ulong id)
                    ? await _dbContext.Collections.FindAsync(id)
                    : _dbContext.Collections.FirstOrDefault(x => x.UserId == ctx.Member.Id && x.Name.Equals(nameOrId));
                if (collection != null && collection.UserId == ctx.Member.Id)
                {
                    IQueryable<DbCollectionItem> items =
                        _dbContext.CollectionItems.Where(x => x.CollectionId == collection.Id);
                    _dbContext.CollectionItems.RemoveRange(items);
                    await _dbContext.SaveChangesAsync();

                    discordEmbed = new DiscordEmbedBuilder()
                        .WithHappyMessage(
                            ctx.Member.DisplayName,
                            string.Format(
                                Resources.ManagingСollectionsCommand_ClearCollection_Cleared,
                                collection.Name));
                }
                else
                {
                    discordEmbed = new DiscordEmbedBuilder()
                        .WithSadMessage(
                            ctx.Member.DisplayName,
                            Resources.ManagingСollectionsCommand_ClearCollection_CollectionNotFound);
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
            IQueryable<DbCollection> collections = _dbContext.Collections.Where(x => x.UserId == ctx.Member.Id);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (DbCollection collection in collections)
            {
                stringBuilder.AppendLine($"{collection.Id}. {collection.Name}");
            }

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(
                    ctx.Member.DisplayName,
                    stringBuilder.Length > 0
                        ? stringBuilder.ToString()
                        : Resources.ManagingСollectionsCommand_ShowCollections_IsEmpty);
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("show-items")]
        [Aliases("items")]
        [Description("Показать последние 25 сообщений коллекции.")]
        public async Task ShowItems(
            CommandContext ctx,
            [Description("Название или Id коллекции"), RemainingText]
            string nameOrId)
        {
            DiscordEmbedBuilder discordEmbed = null;
            if (string.IsNullOrEmpty(nameOrId))
            {
                discordEmbed = new DiscordEmbedBuilder()
                    .WithSadMessage(
                        ctx.Member.DisplayName,
                        Resources.ManagingСollectionsCommand_ShowItems_NameOrIdIsEmpty);
            }
            else
            {
                DbCollection collection = ulong.TryParse(nameOrId, out ulong id)
                    ? await _dbContext.Collections.FindAsync(id)
                    : _dbContext.Collections.FirstOrDefault(
                        x =>
                            x.UserId == ctx.Member.Id && x.Name.Equals(nameOrId));
                if (collection != null && collection.UserId == ctx.Member.Id)
                {
                    // Due to the fact that the EF cannot construct a query to take the last elements, the AsEnumerable
                    // transformation is used, which will execute the query and return an enumeration
                    IEnumerable<DbCollectionItem> items = _dbContext.CollectionItems
                        .Where(x => x.CollectionId == collection.Id).AsEnumerable().TakeLast(25);
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (DbCollectionItem item in items)
                    {
                        stringBuilder.AppendLine(item.MessageId.ToString());
                    }

                    discordEmbed = new DiscordEmbedBuilder()
                        .WithHappyMessage(
                            ctx.Member.DisplayName,
                            stringBuilder.Length != 0
                                ? stringBuilder.ToString()
                                : Resources.ManagingСollectionsCommand_ShowItems_IsEmpty);
                }
                else
                {
                    discordEmbed = new DiscordEmbedBuilder()
                        .WithSadMessage(
                            ctx.Member.DisplayName,
                            Resources.ManagingСollectionsCommand_ShowItems_CollectionNotFound);
                }
            }

            await ctx.RespondAsync(discordEmbed);
        }
        #endregion

        #region NOT COMMAND
        private async Task PrivateDeleteFromCollection(
            CommandContext ctx,
            ulong messageId,
            string nameOrId)
        {
            ulong memberId = ctx.Member.Id;

            DbCollection dbCollection = ulong.TryParse(nameOrId, out ulong id)
                ? await _dbContext.Collections.FindAsync(id)
                : _dbContext.Collections.FirstOrDefault(x => x.Name == nameOrId && x.UserId == memberId);
            if (dbCollection == null || dbCollection.UserId != memberId)
                throw new IncorrectCommandDataException(
                    Resources.ManagingСollectionsCommand_DeleteFromCollection_CollectionNotFound);

            IList<DbCollectionItem> dbCollectionItems = _dbContext.CollectionItems
                .Where(x => x.CollectionId == dbCollection.Id && x.MessageId == messageId).ToList();
            _dbContext.CollectionItems.RemoveRange(dbCollectionItems);
            await _dbContext.SaveChangesAsync();

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(
                    ctx.Member.DisplayName,
                    dbCollectionItems.Count > 0
                        ? string.Format(
                            Resources.ManagingСollectionsCommand_DeleteFromCollection_Deleted,
                            messageId,
                            dbCollection.Name)
                        : string.Format(
                            Resources.ManagingСollectionsCommand_DeleteFromCollection_MessageNotFound,
                            messageId));
            await SendSpecialMessage(ctx, discordEmbed);
        }

        private async Task SaveCollectionItem(
            CommandContext ctx,
            DiscordMessage discordMessage,
            DiscordEmbedBuilder discordEmbed,
            DbCollection dbCollection,
            bool hasPremiumAccess)
        {
            _dbContext.CollectionItems.Add(
                new DbCollectionItem
                {
                    Collection = dbCollection,
                    MessageId = discordMessage.Id,
                    IsSavedLinks = hasPremiumAccess
                });

            DbMessage dbMessage = await _dbContext.Messages.FindAsync(discordMessage.Id);
            if (dbMessage == null)
            {
                dbMessage = new DbMessage
                {
                    Id = discordMessage.Id,
                    ChannelId = ctx.Channel.Id
                };
                _dbContext.Messages.Add(dbMessage);
            }
            dbMessage.Link = string.Join(Constants.LinkSeparator, discordMessage.GetImages(_yukoSettings));

            try
            {
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception)
            {
                _logger.LogError(
                    $"Error saving database changes. Discord info: server {ctx.Guild.Name}; channel {ctx.Channel.Name
                    }; message {discordMessage.Id}.");

                if (ctx.Command.Name.Equals("end"))
                {
                    discordEmbed.WithSadTitle(ctx.Member.DisplayName).WithDescription(
                        string.Format(Resources.ManagingСollectionsCommand_End_ErrorAddingItem, dbCollection.Name));
                }
                else
                {
                    discordEmbed.WithSadMessage(
                        ctx.Member.DisplayName,
                        string.Format(
                            Resources.ManagingСollectionsCommand_AddToCollection_ErrorAddingItem,
                            dbCollection.Name));
                }
            }
        }

        private async Task<DiscordEmbedBuilder> AddToCollection(
            CommandContext ctx,
            DiscordMessage message,
            string nameOrId)
        {
            if (!message.HasImages(_yukoSettings))
                throw new IncorrectCommandDataException(
                    Resources.ManagingСollectionsCommand_AddToCollection_NoAttachments);

            ulong memberId = ctx.Member.Id;

            DbCollection dbCollection = await GetOrCreateCollection(memberId, nameOrId);

            DbCollectionItem dbCollectionItem =
                _dbContext.CollectionItems.FirstOrDefault(
                    x => x.CollectionId == dbCollection.Id && x.MessageId == message.Id);

            if (dbCollectionItem != null)
                throw new IncorrectCommandDataException(string.Format(
                    Resources.ManagingСollectionsCommand_AddToCollection_ExistsInCollection, dbCollection.Name));

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(
                    ctx.Member.DisplayName,
                    string.Format(
                        Resources.ManagingСollectionsCommand_AddToCollection_IsSuccess,
                        dbCollection.Name));

            bool hasPremiumAccess = (await _dbContext.Users.FindAsync(memberId)).HasPremiumAccess;
            await SaveCollectionItem(ctx, message, discordEmbed, dbCollection, hasPremiumAccess);

            return discordEmbed;
        }

        private async Task RenameCollection(
            CommandContext ctx,
            DbCollection dbCollection,
            string newName)
        {
            if (dbCollection == null)
                throw new IncorrectCommandDataException(
                    Resources.ManagingСollectionsCommand_RenameCollection_CollectionNotFound);

            ulong memberId = ctx.Member.Id;
            if (_dbContext.Collections.FirstOrDefault(x => x.Name.Equals(newName) && x.UserId == memberId) != null)
                throw new IncorrectCommandDataException(
                    Resources.ManagingСollectionsCommand_RenameCollection_Exists);

            string message = string.Format(
                Resources.ManagingСollectionsCommand_RenameCollection_Renamed,
                dbCollection.Name, newName);
            dbCollection.Name = newName;
            await _dbContext.SaveChangesAsync();

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(ctx.Member.DisplayName, message);
            await ctx.RespondAsync(discordEmbed);
        }

        private async Task<DbCollection> GetOrCreateCollection(ulong memberId, string nameOrId)
        {
            DbCollection dbCollection = ulong.TryParse(nameOrId, out ulong id)
                ? await _dbContext.Collections.FindAsync(id)
                : _dbContext.Collections.FirstOrDefault(x => x.Name == nameOrId && x.UserId == memberId);
            if (dbCollection == null)
            {
                if (DefaultCollection.Equals(nameOrId, StringComparison.OrdinalIgnoreCase))
                {
                    dbCollection = new DbCollection
                    {
                        UserId = memberId,
                        Name = DefaultCollection
                    };
                    _dbContext.Collections.Add(dbCollection);
                    await _dbContext.SaveChangesAsync();
                }
                else
                {
                    throw new IncorrectCommandDataException(
                        Resources.ManagingСollectionsCommand_AddToCollection_CollectionNotFound);
                }
            }
            return dbCollection;
        }

        private async Task SendSpecialMessage(CommandContext ctx, DiscordEmbedBuilder embed)
        {
            DbGuildSettings guildSettings = await _dbContext.GuildsSettings.FindAsync(ctx.Guild.Id);
            if (guildSettings != null && !guildSettings.AddCommandResponse)
            {
                bool send = embed.Color.Value.Value == DiscordColor.Red.Value;
                if (!send)
                {
                    DbUser dbUser = await _dbContext.Users.FindAsync(ctx.Member.Id);
                    send = dbUser.InfoMessages;
                }
                if (send)
                {
                    DiscordDmChannel dmChannel = await ctx.Member.CreateDmChannelAsync();
                    DiscordMessage discordMessage = await dmChannel.SendMessageAsync(embed);
                    await discordMessage.CreateReactionAsync(
                        DiscordEmoji.FromName(
                            ctx.Client,
                            Constants.DeleteMessageEmoji,
                            false));
                }
            }
            else
            {
                await ctx.RespondAsync(embed);
            }
        }
        #endregion
    }
}