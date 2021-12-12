using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;
using YukoBot.Commands.Attribute;
using YukoBot.Models.Database;

namespace YukoBot.Commands
{
    [RequireRegisteredAndNoBan]
    public class ManagingСollectionsCommandModule : CommandModule
    {
        private const string DefaultCollection = "Default";

        public ManagingСollectionsCommandModule()
        {
            ModuleName = "Управление коллекциями";
        }

        #region Command: add
        [Command("add")]
        [Description("Добавляет сообщение в коллекцию по умолчанию")]
        public async Task AddToCollection(CommandContext commandContext)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{commandContext.Member.DisplayName}");
            DiscordMessage message = commandContext.Message.Reference.Message;
            bool isAttacments = (message.Attachments.Count > 0) || message.Embeds.Any(x => x.Url.IsFile);
            if (isAttacments)
            {
                YukoDbContext db = new YukoDbContext();
                ulong memberId = commandContext.Member.Id;
                DbCollection dbCollection = db.Collections.Where(x => x.UserId == memberId).FirstOrDefault();
                if (dbCollection == null)
                {
                    dbCollection = new DbCollection
                    {
                        UserId = memberId,
                        Name = DefaultCollection
                    };
                    db.Collections.Add(dbCollection);
                    await db.SaveChangesAsync();
                }
                DbCollectionItem dbCollectionItem = db.CollectionItems.Where(x => x.CollectionId == dbCollection.Id && x.MessageId == message.Id).FirstOrDefault();
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
                        MessageId = message.Id,
                        ChannelId = message.Channel.Id,
                        CollectionId = dbCollection.Id
                    };
                    db.CollectionItems.Add(dbCollectionItem);
                    await db.SaveChangesAsync();
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
            await commandContext.RespondAsync(discordEmbed);
        }

        //[Command("add")]
        //[Description("Добавляет изображения из сообщения в указанную коллекцию коллекцию")]
        //public async Task AddToCollection(CommandContext commandContext, string collectionName)
        //{
        //    await commandContext.RespondAsync("InProgress");
        //}

        //[Command("add")]
        //[Description("Добавляет изображения из сообщения в указанную коллекцию коллекцию")]
        //public async Task AddToCollection(CommandContext commandContext, ulong collectionId)
        //{
        //    await commandContext.RespondAsync("InProgress");
        //}
        #endregion

        #region Command: add-by-id
        [Command("add-by-id")]
        [Description("Добавляет сообщение в коллекцию по умолчанию")]
        public async Task AddToCollectionById(CommandContext commandContext, ulong messageId)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{commandContext.Member.DisplayName}");
            DiscordMessage message = await commandContext.Channel.GetMessageAsync(messageId);
            bool isAttacments = (message.Attachments.Count > 0) || message.Embeds.Any(x => x.Url.IsFile);
            if (isAttacments)
            {
                YukoDbContext db = new YukoDbContext();
                ulong memberId = commandContext.Member.Id;
                DbCollection dbCollection = db.Collections.Where(x => x.UserId == memberId).FirstOrDefault();
                if (dbCollection == null)
                {
                    dbCollection = new DbCollection
                    {
                        UserId = memberId,
                        Name = DefaultCollection
                    };
                    db.Collections.Add(dbCollection);
                    await db.SaveChangesAsync();
                }
                DbCollectionItem dbCollectionItem = db.CollectionItems.Where(x => x.CollectionId == dbCollection.Id && x.MessageId == message.Id).FirstOrDefault();
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
                        MessageId = message.Id,
                        CollectionId = dbCollection.Id
                    };
                    db.CollectionItems.Add(dbCollectionItem);
                    await db.SaveChangesAsync();
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
            await commandContext.RespondAsync(discordEmbed);
        }

        //[Command("add-by-id")]
        //[Description("Добавляет изображения из указанного сообщения в дефолтную коллекцию")]
        //public async Task AddToCollectionById(CommandContext commandContext, ulong messageId, string collectionName)
        //{
        //    await commandContext.RespondAsync("InProgress");
        //}

        //[Command("add-by-id")]
        //[Description("Добавляет изображения из указанного сообщения в дефолтную коллекцию")]
        //public async Task AddToCollectionById(CommandContext commandContext, ulong messageId, ulong collectionId)
        //{
        //    await commandContext.RespondAsync("InProgress");
        //}
        #endregion

        //#region Command: delete
        //[Command("delete")]
        //[Description("delete")]
        //public async Task DeleteFromCollection(CommandContext commandContext, ulong messageId)
        //{
        //    await commandContext.RespondAsync("InProgress");
        //}

        //[Command("delete")]
        //[Description("delete")]
        //public async Task DeleteFromCollection(CommandContext commandContext, ulong messageId, string collectionName)
        //{
        //    await commandContext.RespondAsync("InProgress");
        //}

        //[Command("delete")]
        //[Description("delete")]
        //public async Task DeleteFromCollection(CommandContext commandContext, ulong messageId, ulong collectionId)
        //{
        //    await commandContext.RespondAsync("InProgress");
        //}
        //#endregion

        //#region Command: *-collection
        //[Command("add-collection")]
        //[Description("add-collection")]
        //public async Task AddCollection(CommandContext commandContext, string collectionName)
        //{
        //    await commandContext.RespondAsync("InProgress");
        //}

        //[Command("delete-collection")]
        //[Description("delete-collection")]
        //public async Task DeleteCollection(CommandContext commandContext, string collectionName)
        //{
        //    await commandContext.RespondAsync("InProgress");
        //}

        //[Command("delete-collection")]
        //[Description("delete-collection")]
        //public async Task DeleteCollection(CommandContext commandContext, ulong collectionId)
        //{
        //    await commandContext.RespondAsync("InProgress");
        //}
        //#endregion

        //#region show-*
        //[Command("show-collections")]
        //[Description("show-collections")]
        //public async Task ShowCollections(CommandContext commandContext)
        //{
        //    await commandContext.RespondAsync("InProgress");
        //}

        //[Command("show")]
        //[Description("show")]
        //public async Task ShowItems(CommandContext commandContext, ulong collectionId)
        //{
        //    await commandContext.RespondAsync("InProgress");
        //}

        //[Command("show")]
        //[Description("show")]
        //public async Task ShowItems(CommandContext commandContext, string collectionName)
        //{
        //    await commandContext.RespondAsync("InProgress");
        //}
        //#endregion
    }
}