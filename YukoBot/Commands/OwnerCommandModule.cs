using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Threading.Tasks;

namespace YukoBot.Commands
{
    [RequireOwner]
    public class OwnerCommandModule : CommandModule
    {
        public OwnerCommandModule()
        {
            ModuleName = "Команды управления";
        }

        [Command("shutdown")]
        [Aliases("sd")]
        [Description("Выключить бота")]
        public async Task Shutdown(CommandContext commandContext)
        {
            await commandContext.RespondAsync("Ok");
            YukoBot.Current.Shutdown();
        }

        [Command("active-time")]
        [Description("Время работы бота")]
        public async Task ActiveTime(CommandContext commandContext)
        {
            TimeSpan timeSpan = DateTime.Now - YukoBot.Current.StartDateTime;
            await commandContext.RespondAsync($"{timeSpan.Days}:{timeSpan.Hours}:{timeSpan.Minutes}:{timeSpan.Seconds}");
        }

        //[Command("remove-record-db")]
        //[Aliases("rdb")]
        //[Description("Удаляет запись из таблицы в БД")]
        //public async Task RemoveRecordFromDb(CommandContext commandContext,
        //    [Description("Участник сервера (гильдии)")] DiscordMember discordMember,
        //    [Description("Таблица в БД"), ArgumentValues("users/u", "bans/b")] string table,
        //    [Description("Дополнительные параметры"), ArgumentValues("all/a")] string flags = "")
        //{
        //    table = table.ToLower();
        //    if (table.Equals("users") || table.Equals("u") ||
        //        table.Equals("bans") || table.Equals("b"))
        //    {
        //        YukoDbContext database = new YukoDbContext();
        //        if (table.Equals("users") || table.Equals("u"))
        //        {
        //            if (database.Users.Remove(new DbUser { Id = discordMember.Id }) != null)
        //            {
        //                database.SaveChanges();
        //            }
        //        }
        //        else
        //        {
        //            if (flags.Equals("all") || flags.Equals("a"))
        //            {
        //                IQueryable<DbBan> bans = database.Bans.Where(x => x.UserId == discordMember.Id);
        //                if (bans.FirstOrDefault() != null)
        //                {
        //                    database.Bans.RemoveRange(bans);
        //                    database.SaveChanges();
        //                }
        //            }
        //            else
        //            {
        //                DbBan ban = database.Bans.Where(x => x.UserId == discordMember.Id && x.ServerId == commandContext.Guild.Id).FirstOrDefault();
        //                if (ban != null)
        //                {
        //                    database.Bans.Remove(ban);
        //                    database.SaveChanges();
        //                }
        //            }
        //        }
        //        await commandContext.RespondAsync("Ок");
        //    }
        //    else
        //    {
        //        await commandContext.RespondAsync("Такой таблицы не существует");
        //    }
        //}
    }
}