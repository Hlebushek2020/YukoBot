using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Linq;
using System.Threading.Tasks;
using YukoBot.Models.Database;

namespace YukoBot.Commands
{
    public class ExperimentalCommandModule : CommandModule
    {
        public ExperimentalCommandModule()
        {
            ModuleName = "УПРАВЛЕНИЕ КОЛЛЕКЦИЯМИ (EXP)";
        }

        [Command("add")]
        [Description("Добавляет изображения из сообщения в дефолтную коллекцию")]
        public async Task AddToCollection(CommandContext commandContext)
        {
            DiscordMessage message = commandContext.Message.Reference.Message;
            ulong memberId = commandContext.Member.Id;
            YukoDbContext yukoCtx = new YukoDbContext();
            if (yukoCtx.Users.Find(memberId) == null)
            {
                await commandContext.RespondAsync("User not found");
                return;
            }
            DbCollection dbCollection = yukoCtx.Collections.Where(x => x.UserId == memberId).FirstOrDefault();
            if (dbCollection == null)
            {
                dbCollection = new DbCollection();
                dbCollection.UserId = memberId;
                dbCollection.Name = "Default";
                yukoCtx.Collections.Add(dbCollection);
                await yukoCtx.SaveChangesAsync();
            }
            DbCollectionItem dbCollectionItem = yukoCtx.CollectionItems.Where(x => x.CollectionId == dbCollection.Id && x.MessageId == message.Id).FirstOrDefault();
            if (dbCollectionItem != null)
            {
                await commandContext.RespondAsync("Item found");
                return;
            }
            dbCollectionItem = new DbCollectionItem();
            dbCollectionItem.MessageId = message.Id;
            dbCollectionItem.CollectionId = dbCollection.Id;
            yukoCtx.CollectionItems.Add(dbCollectionItem);
            await yukoCtx.SaveChangesAsync();
            await commandContext.RespondAsync("Ok");
        }
    }
}
