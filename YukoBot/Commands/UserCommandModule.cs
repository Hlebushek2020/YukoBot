using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using YukoBot.Commands.Models;
using YukoBot.Extensions;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Commands
{
    public class UserCommandModule : CommandModule
    {
        private readonly IYukoSettings _yukoSettings;
        private readonly ILogger<UserCommandModule> _logger;

        private string BotDescription { get; }

        public UserCommandModule(IYukoSettings yukoSettings, ILogger<UserCommandModule> logger)
            : base(Categories.User, null)
        {
            _yukoSettings = yukoSettings;
            _logger = logger;
            BotDescription = $"Привет, для того что бы узнать больше информации обо мне выполни команду `{
                _yukoSettings.BotPrefix}info`.";
        }

        [Command("register")]
        [Aliases("reg")]
        [Description(
            "Зарегистрироваться и получить пароль и логин от своей учетной записи или сбросить текущий пароль.")]
        public async Task Register(CommandContext ctx)
        {
            YukoDbContext dbCtx = new YukoDbContext(_yukoSettings);
            DbUser dbUser = await dbCtx.Users.FindAsync(ctx.User.Id);
            bool isRegister = false;
            if (dbUser == null)
            {
                isRegister = true;
                dbUser = new DbUser
                {
                    Id = ctx.User.Id,
                    Nikname = ctx.User.Username //+ "#" + ctx.User.Discriminator
                };
                dbCtx.Users.Add(dbUser);
            }

            string password = "";
            Random random = new Random();
            while (password.Length != 10)
            {
                password += (char) random.Next(33, 127);
            }

            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder hashBuilder = new StringBuilder(hashBytes.Length / 2);
                foreach (byte code in hashBytes)
                {
                    hashBuilder.Append(code.ToString("X2"));
                }
                dbUser.Password = hashBuilder.ToString();
            }

            await dbCtx.SaveChangesAsync();

            DiscordDmChannel userChat = await ctx.Member.CreateDmChannelAsync();
            DiscordEmbedBuilder discordEmbedDm = new DiscordEmbedBuilder()
                .WithHappyTitle(isRegister ? "Регистрация прошла успешно!" : "Пароль сменен!")
                .WithColor(Constants.SuccessColor)
                .AddField("Логин", $"Используй **{dbUser.Nikname}** или **{dbUser.Id}**")
                .AddField(isRegister ? "Пароль" : "Новый пароль", password);
            DiscordMessage userMessage = await userChat.SendMessageAsync(discordEmbedDm);
            await userMessage.CreateReactionAsync(
                DiscordEmoji.FromName(ctx.Client, Constants.DeleteMessageEmoji, false));

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(
                    ctx.Member.DisplayName,
                    isRegister
                        ? "Регистрация прошла успешно! Пароль и логин от учетной записи отправлены в ЛС."
                        : "Новый пароль от учетной записи отправлен в ЛС.");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("help")]
        [Description(
            "Показать список команд и категорий, если для команды не указан аргумент. Если в качестве аргумента указана категория - показывает список комманд этой категории с их описанием, если указана команда - показывает ее полное описание.")]
        public async Task Help(
            CommandContext ctx,
            [Description("Категория или команда")]
            string categoryOrCommand = "")
        {
            if (!string.IsNullOrWhiteSpace(categoryOrCommand))
            {
                if (CheckHelpCategoryCommand(categoryOrCommand))
                {
                    IEnumerable<Command> commands = ctx.CommandsNext.RegisteredCommands.Values.Distinct()
                        .Where(
                            x => ((x.Module as SingletonCommandModule).Instance as CommandModule).Category
                                 .HelpCommand.Equals(categoryOrCommand) &&
                                 !x.IsHidden && !x.RunChecksAsync(ctx, true).Result.Any());

                    SortedDictionary<string, string> descriptionByCommand = new SortedDictionary<string, string>();

                    foreach (Command command in commands)
                    {
                        string aliases = string.Empty;
                        if (command.Aliases.Count > 0)
                            aliases = $" ({string.Join(' ', command.Aliases)})";

                        string fieldTitle = $"{command.Name}{aliases}";

                        descriptionByCommand.Add(fieldTitle, command.Description);
                    }

                    DiscordEmbedBuilder embed;

                    Category category = GetCategoryByHelpCommand(categoryOrCommand);

                    if (descriptionByCommand.Count > 0)
                    {
                        embed = new DiscordEmbedBuilder()
                            .WithHappyMessage($"{_yukoSettings.BotPrefix} |", BotDescription)
                            .WithFooter($"v{Program.Version}")
                            .AddField(category.Name, new string('=', category.Name.Length));

                        foreach (KeyValuePair<string, string> item in descriptionByCommand)
                        {
                            embed.AddField(item.Key, item.Value);
                        }
                    }
                    else
                    {
                        embed = new DiscordEmbedBuilder()
                            .WithSadMessage(ctx.Member.DisplayName, category.AccessError);
                    }

                    await ctx.RespondAsync(embed);
                }
                else
                {
                    Command command = ctx.CommandsNext.FindCommand(categoryOrCommand, out string args);

                    if (command == null)
                        throw new CommandNotFoundException(categoryOrCommand);

                    IEnumerable<CheckBaseAttribute> failedChecks = await command.RunChecksAsync(ctx, true);
                    if (failedChecks.Any())
                        throw new ChecksFailedException(command, ctx, failedChecks);

                    StringBuilder descriptionBuilder = new StringBuilder();

                    bool countOverloads = command.Overloads.Count > 1;

                    for (int i = 0; i < command.Overloads.Count; i++)
                    {
                        CommandOverload commandOverload = command.Overloads[i];

                        if (countOverloads)
                        {
                            descriptionBuilder.AppendLine($"**__Вариант {i + 1}__**");
                        }

                        descriptionBuilder
                            .AppendLine(
                                $"```\n{_yukoSettings.BotPrefix} {command.Name} {string.Join(
                                    ' ',
                                    commandOverload.Arguments.Select(x => $"[{x.Name}]").ToList())}```{
                                    command.Description}")
                            .AppendLine();

                        if (command.Aliases?.Count != 0)
                        {
                            descriptionBuilder.AppendLine("**Алиасы:**");
                            foreach (string alias in command.Aliases)
                            {
                                descriptionBuilder.Append($"{alias} ");
                            }
                            descriptionBuilder.AppendLine().AppendLine();
                        }

                        if (commandOverload.Arguments.Count != 0)
                        {
                            descriptionBuilder.AppendLine("**Аргументы:**");
                            foreach (CommandArgument argument in commandOverload.Arguments)
                            {
                                string defaultValue = (argument.DefaultValue != null)
                                    ? $" (Необязательно, по умолчанию: {argument.DefaultValue})"
                                    : string.Empty;
                                descriptionBuilder.AppendLine(
                                    $"`{argument.Name}`: {argument.Description}{defaultValue}");
                            }
                            descriptionBuilder.AppendLine();
                        }
                    }

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                        .WithHappyMessage($"{command.Name} |", descriptionBuilder.ToString())
                        .WithFooter($"v{Program.Version}");

                    await ctx.RespondAsync(embed);
                }
            }
            else
            {
                IEnumerable<Command> commands = ctx.CommandsNext.RegisteredCommands.Values.Distinct()
                    .Where(x => !x.IsHidden && !x.RunChecksAsync(ctx, true).Result.Any());

                Dictionary<string, SortedSet<string>> sortedCommandsByCategory =
                    new Dictionary<string, SortedSet<string>>();

                foreach (Command command in commands)
                {
                    CommandModule yukoModule = (command.Module as SingletonCommandModule).Instance as CommandModule;

                    string categoryName = yukoModule.Category.Name;

                    if (string.IsNullOrEmpty(categoryName))
                        continue;

                    if (!sortedCommandsByCategory.ContainsKey(categoryName))
                        sortedCommandsByCategory.Add(categoryName, new SortedSet<string>());

                    string aliases = string.Empty;
                    if (command.Aliases.Count > 0)
                        aliases = $" ({string.Join(' ', command.Aliases)})";

                    sortedCommandsByCategory[categoryName].Add($"`{command.Name}{aliases}`");
                }

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithHappyMessage($"{_yukoSettings.BotPrefix} |", BotDescription)
                    .WithFooter($"v{Program.Version}");

                foreach (Category mInfo in GetCategories())
                {
                    string categoryName = mInfo.Name;
                    string fieldName = $"{categoryName} (help {mInfo.HelpCommand})";
                    embed.AddField(
                        fieldName,
                        sortedCommandsByCategory.TryGetValue(categoryName, out var sortedCommands)
                            ? string.Join(' ', sortedCommands)
                            : mInfo.AccessError);
                }

                await ctx.RespondAsync(embed);
            }
        }

        [Command("info")]
        [Description("Информация о боте и его возможностях.")]
        public async Task Info(CommandContext ctx)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(
                    _yukoSettings.BotPrefix + " | Info",
                    "Привет, я Юко. Бот созданный для быстрого скачивания картинок с каналов серверов (гильдий) " +
                    "дискорда. Так же я могу составлять коллекции из сообщений с картинками (или ссылками на картинки) " +
                    "для последующего скачивания этих коллекций.")
                .AddField(
                    "Управление коллекциями",
                    "Данный функционал доступен только зарегистрированным пользователям. Для просмотра всех доступных " +
                    "команд управления коллекциями воспользуйся командой `" + _yukoSettings.BotPrefix + "help " +
                    Categories.CollectionManagement.HelpCommand + "`.")
                .AddField(
                    "Премиум доступ",
                    "Премиум доступ позволяет заранее сохранять необходимые данные (при добавлении сообщения в коллекцию) " +
                    "для скачивания вложений из сообщения. Это в разы уменьшает время получения ссылок клиентом для " +
                    "скачивания вложений. На данный момент выдается моим хозяином.")
                .AddField(
                    "Ссылки",
                    "[GitHub](https://github.com/Hlebushek2020/YukoBot) | [Discord](https://discord.gg/a2EZmbaxT9)")
                .WithThumbnail(ctx.Client.CurrentUser.AvatarUrl, 50, 50)
                .WithFooter($"v{Program.Version}");

            await ctx.RespondAsync(discordEmbed);
        }

        [Command("avatar")]
        [Aliases("ava")]
        [Description("Получить аватар пользователя")]
        public async Task Avatar(
            CommandContext ctx,
            [Description("Участник сервера")]
            DiscordMember member)
        {
            YukoDbContext dbCtx = new YukoDbContext(_yukoSettings);
            DbUser user = await dbCtx.Users.FindAsync(ctx.Member.Id);

            bool isUseMessage = user != null && user.HasPremiumAccess;
            if (isUseMessage)
            {
                try
                {
                    using HttpClient client = new HttpClient();
                    Stream fileStream = await client.GetStreamAsync(member.AvatarUrl);
                    string fileName = Path.GetFileName(member.AvatarUrl) ?? Guid.NewGuid().ToString();
                    if (fileName.Contains('?'))
                        fileName = fileName.Remove(fileName.IndexOf('?'));

                    DiscordMessageBuilder discordMessage = new DiscordMessageBuilder()
                        .AddFile(fileName, fileStream);

                    await ctx.RespondAsync(discordMessage);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Failed to download attachment. Exception: {ex.Message}.");
                    isUseMessage = false;
                }
            }

            if (!isUseMessage)
            {
                DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                    .WithHappyTitle(ctx.Member.DisplayName)
                    .WithColor(Constants.SuccessColor)
                    .WithImageUrl(member.AvatarUrl);

                await ctx.RespondAsync(discordEmbed);
            }
        }
    }
}