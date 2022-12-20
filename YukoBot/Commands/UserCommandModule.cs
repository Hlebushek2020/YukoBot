using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using YukoBot.Commands.Models;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;

namespace YukoBot.Commands
{
    public class UserCommandModule : CommandModule
    {
        public UserCommandModule() : base(Categories.User) { }

        [Command("register")]
        [Aliases("reg")]
        [Description("Зарегистрироваться и получить пароль и логин от своей учетной записи или сбросить текущий пароль")]
        public async Task Register(CommandContext ctx)
        {
            DiscordMember member = ctx.Member;
            DiscordUser user = ctx.User;

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle(ctx.Member.DisplayName);

            YukoDbContext dbCtx = new YukoDbContext();
            DbUser dbUser = dbCtx.Users.Find(user.Id);
            if (dbUser != null)
            {
                discordEmbed
                    .WithColor(Constants.SuccessColor)
                    .WithDescription($"Вы уже зарегистрированы! {Constants.HappySmile}");
                await ctx.RespondAsync(discordEmbed);
                return;
            }
            dbUser = new DbUser
            {
                Id = user.Id,
                Nikname = user.Username + "#" + user.Discriminator
            };

            string password = "";
            Random random = new Random();
            while (password.Length != 10)
            {
                password += (char)random.Next(33, 127);
            }

            using (SHA256CryptoServiceProvider sha256 = new SHA256CryptoServiceProvider())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder hashBuilder = new StringBuilder(hashBytes.Length / 2);
                foreach (byte code in hashBytes)
                {
                    hashBuilder.Append(code.ToString("X2"));
                }
                dbUser.Password = hashBuilder.ToString();
            }

            dbCtx.Users.Add(dbUser);
            await dbCtx.SaveChangesAsync();

            DiscordDmChannel userChat = await member.CreateDmChannelAsync();
            DiscordEmbedBuilder discordEmbedDm = new DiscordEmbedBuilder()
                .WithColor(Constants.SuccessColor)
                .WithTitle($"Регистрация прошла успешно! {Constants.HappySmile}")
                .AddField("Логин", $"Используй **{dbUser.Nikname}** или **{dbUser.Id}**")
                .AddField("Пароль", password);
            DiscordMessage userMessage = await userChat.SendMessageAsync(discordEmbedDm);
            await userMessage.CreateReactionAsync(DiscordEmoji.FromName(ctx.Client, Constants.DeleteMessageEmoji, false));

            discordEmbed
                .WithColor(Constants.SuccessColor)
                .WithDescription($"Регистрация прошла успешно! Пароль для входа отправлен в личные сообщения. {Constants.HappySmile}");
            await ctx.RespondAsync(discordEmbed);
        }

        [Command("help")]
        [Description("Показать список команд и категорий, если для команды не указан аргумент. Если в качестве аргумента указана категория - показывает список комманд этой категории с их описанием, если указана команда - показывает ее полное описание")]
        public async Task Help(CommandContext ctx,
            [Description("Категория или команда")] string categoryOrCommand = null)
        {
            if (categoryOrCommand != null)
            {
                if (CheckHelpCategoryCommand(categoryOrCommand))
                {
                    IEnumerable<Command> commands = ctx.CommandsNext.RegisteredCommands.Values.Distinct()
                        .Where(x => ((x.Module as SingletonCommandModule).Instance as CommandModule).Category.HelpCommand.Equals(categoryOrCommand) &&
                            !x.IsHidden && !x.RunChecksAsync(ctx, true).Result.Any());

                    List<string[]> commandOfDescription = new List<string[]>();

                    foreach (Command command in commands)
                    {
                        CommandModule yukoModule = (command.Module as SingletonCommandModule).Instance as CommandModule;

                        string aliases = string.Empty;
                        if (command.Aliases.Count > 0)
                            aliases = $" ({string.Join(' ', command.Aliases)})";

                        string fieldTitle = $"{command.Name}{aliases}";

                        commandOfDescription.Add(new string[] { fieldTitle, command.Description });
                    }

                    DiscordEmbedBuilder embed;

                    Category category = GetCategoryByHelpCommand(categoryOrCommand);

                    if (commandOfDescription.Count > 0)
                    {
                        embed = new DiscordEmbedBuilder()
                            .WithTitle($"{Settings.BotPrefix} | {Constants.HappySmile}")
                            .WithColor(Constants.SuccessColor)
                            .WithFooter($"v{Assembly.GetExecutingAssembly().GetName().Version}")
                            .WithDescription(Settings.BotDescription)
                            .AddField(category.Name, new string('=', category.Name.Length));

                        foreach (string[] item in commandOfDescription)
                        {
                            embed.AddField(item[0], item[1]);
                        }
                    }
                    else
                    {
                        embed = new DiscordEmbedBuilder()
                            .WithTitle(ctx.Member.DisplayName)
                            .WithColor(Constants.ErrorColor)
                            .WithDescription(category.AccessError);
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

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                        .WithTitle($"{command.Name} | {Constants.HappySmile}")
                        .WithColor(Constants.SuccessColor)
                        .WithFooter($"v{Assembly.GetExecutingAssembly().GetName().Version}");

                    StringBuilder descriptionBuilder = new StringBuilder();

                    bool countOverloads = command.Overloads.Count > 1;

                    for (int i = 0; i < command.Overloads.Count; i++)
                    {
                        CommandOverload commandOverload = command.Overloads[i];

                        if (countOverloads)
                        {
                            descriptionBuilder.AppendLine($"**__Вариант {i + 1}__**");
                        }

                        descriptionBuilder.AppendLine($"```\n{Settings.BotPrefix} {command.Name} {string.Join(' ', commandOverload.Arguments.Select(x => $"[{x.Name}]").ToList())}```{command.Description}").AppendLine();

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
                                string defaultValue = (argument.DefaultValue != null) ? $" (По умолчанию: {argument.DefaultValue})" : string.Empty;
                                descriptionBuilder.AppendLine($"`{argument.Name}`: {argument.Description}{defaultValue}");
                            }
                            descriptionBuilder.AppendLine();
                        }
                    }

                    embed.WithDescription(descriptionBuilder.ToString());

                    await ctx.RespondAsync(embed);
                }
            }
            else
            {
                IEnumerable<Command> commands = ctx.CommandsNext.RegisteredCommands.Values.Distinct()
                    .Where(x => !x.IsHidden && !x.RunChecksAsync(ctx, true).Result.Any());

                Dictionary<string, List<string>> sortedCommands = new Dictionary<string, List<string>>();

                foreach (Command command in commands)
                {
                    CommandModule yukoModule = (command.Module as SingletonCommandModule).Instance as CommandModule;

                    string categoryName = yukoModule.Category.Name;

                    if (string.IsNullOrEmpty(categoryName))
                        continue;

                    if (!sortedCommands.ContainsKey(categoryName))
                        sortedCommands.Add(categoryName, new List<string>());

                    string aliases = string.Empty;
                    if (command.Aliases.Count > 0)
                        aliases = $" ({string.Join(' ', command.Aliases)})";

                    sortedCommands[categoryName].Add($"`{command.Name}{aliases}`");
                }

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    .WithTitle($"{Settings.BotPrefix} | {Constants.HappySmile}")
                    .WithColor(Constants.SuccessColor)
                    .WithFooter($"v{Assembly.GetExecutingAssembly().GetName().Version}")
                    .WithDescription(Settings.BotDescription);

                foreach (Category mInfo in GetCategories())
                {
                    string categoryName = mInfo.Name;
                    string fieldName = $"{categoryName} (help {mInfo.HelpCommand})";
                    if (sortedCommands.ContainsKey(categoryName))
                    {
                        embed.AddField(fieldName, string.Join(' ', sortedCommands[categoryName]));
                    }
                    else
                    {
                        embed.AddField(fieldName, mInfo.AccessError);
                    }
                }

                await ctx.RespondAsync(embed);
            }
        }
    }
}