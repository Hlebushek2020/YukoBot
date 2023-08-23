using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using YukoBot.Commands.Models;
using YukoBot.Extensions;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Commands
{
    public class UserCommandModule : CommandModule
    {
        private readonly IYukoSettings _yukoSettings;

        private string BotDescription { get; }

        public UserCommandModule(IYukoSettings yukoSettings) : base(Categories.User, null)
        {
            _yukoSettings = yukoSettings;
            BotDescription = string.Format(Resources.BotDescription, _yukoSettings.BotPrefix);
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
                    Nikname = ctx.User.Username
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
                .WithHappyTitle(isRegister
                    ? Resources.UserCommand_Register_DmTitle_Register
                    : Resources.UserCommand_Register_DmTitle_PasswordChange)
                .WithColor(Constants.SuccessColor)
                .AddField(Resources.UserCommand_Register_FieldDmTitle_Login,
                    string.Format(Resources.UserCommand_Register_FieldDmDescription_Login, dbUser.Nikname, dbUser.Id))
                .AddField(
                    isRegister
                        ? Resources.UserCommand_Register_FieldDmTitle_Password
                        : Resources.UserCommand_Register_FieldDmTitle_NewPassword, password);
            DiscordMessage userMessage = await userChat.SendMessageAsync(discordEmbedDm);
            await userMessage.CreateReactionAsync(
                DiscordEmoji.FromName(ctx.Client, Constants.DeleteMessageEmoji, false));

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithHappyMessage(
                    ctx.Member.DisplayName,
                    isRegister
                        ? Resources.UserCommand_Register_Description_Register
                        : Resources.UserCommand_Register_Description_PasswordChange);
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

                    Dictionary<string, string> commandOfDescription = new Dictionary<string, string>();

                    foreach (Command command in commands)
                    {
                        string aliases = string.Empty;
                        if (command.Aliases.Count > 0)
                            aliases = $" ({string.Join(' ', command.Aliases)})";

                        string fieldTitle = $"{command.Name}{aliases}";

                        commandOfDescription.Add(fieldTitle, command.Description);
                    }

                    DiscordEmbedBuilder embed;

                    Category category = GetCategoryByHelpCommand(categoryOrCommand);

                    if (commandOfDescription.Count > 0)
                    {
                        embed = new DiscordEmbedBuilder()
                            .WithHappyMessage($"{_yukoSettings.BotPrefix} |", BotDescription)
                            .WithFooter($"v{Program.Version}")
                            .AddField(category.Name, new string('=', category.Name.Length));

                        foreach (KeyValuePair<string, string> keyValuePair in commandOfDescription)
                        {
                            embed.AddField(keyValuePair.Key, keyValuePair.Value);
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

                    string resOptionsSection = Resources.UserCommand_Help_OptionsSection;
                    string resAliasesSection = Resources.UserCommand_Help_AliasesSection;
                    string resArgumentsSection = Resources.UserCommand_Help_ArgumentsSection;
                    string resOptionalArgument = Resources.UserCommand_Help_OptionalArgument;

                    for (int i = 0; i < command.Overloads.Count; i++)
                    {
                        CommandOverload commandOverload = command.Overloads[i];

                        if (countOverloads)
                            descriptionBuilder.AppendFormat(resOptionsSection, i + 1);

                        descriptionBuilder
                            .AppendLine(
                                $"```\n{_yukoSettings.BotPrefix} {command.Name} {string.Join(
                                    ' ',
                                    commandOverload.Arguments.Select(x => $"[{x.Name}]").ToList())}```{
                                    command.Description}")
                            .AppendLine();

                        if (command.Aliases?.Count != 0)
                        {
                            descriptionBuilder.AppendLine(resAliasesSection);
                            foreach (string alias in command.Aliases)
                            {
                                descriptionBuilder.Append($"{alias} ");
                            }
                            descriptionBuilder.AppendLine().AppendLine();
                        }

                        if (commandOverload.Arguments.Count != 0)
                        {
                            descriptionBuilder.AppendLine(resArgumentsSection);
                            foreach (CommandArgument argument in commandOverload.Arguments)
                            {
                                string defaultValue = (argument.DefaultValue != null)
                                    ? string.Format(resOptionalArgument, argument.DefaultValue)
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

                Dictionary<string, HashSet<string>> sortedCommands = new Dictionary<string, HashSet<string>>();

                foreach (Command command in commands)
                {
                    CommandModule yukoModule = (command.Module as SingletonCommandModule).Instance as CommandModule;

                    string categoryName = yukoModule.Category.Name;

                    if (string.IsNullOrEmpty(categoryName))
                        continue;

                    if (!sortedCommands.ContainsKey(categoryName))
                        sortedCommands.Add(categoryName, new HashSet<string>());

                    string aliases = string.Empty;
                    if (command.Aliases.Count > 0)
                        aliases = $" ({string.Join(' ', command.Aliases)})";

                    sortedCommands[categoryName].Add($"`{command.Name}{aliases}`");
                }

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithHappyMessage($"{_yukoSettings.BotPrefix} |", BotDescription)
                    .WithFooter($"v{Program.Version}");

                foreach (Category mInfo in GetCategories())
                {
                    string categoryName = mInfo.Name;
                    string fieldName = $"{categoryName} (help {mInfo.HelpCommand})";
                    embed.AddField(fieldName,
                        sortedCommands.TryGetValue(categoryName, out HashSet<string> value)
                            ? string.Join(' ', value)
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
    }
}