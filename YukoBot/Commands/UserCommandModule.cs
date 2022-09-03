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
        public UserCommandModule() : base(Category.User) { }

        [Command("register")]
        [Aliases("reg")]
        [Description("Регистрация")]
        public async Task Register(CommandContext commandContext)
        {
            DiscordMember member = commandContext.Member;
            DiscordUser user = commandContext.User;

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{member.DisplayName}");

            YukoDbContext dbContext = new YukoDbContext();
            DbUser dbUser = dbContext.Users.Find(user.Id);
            if (dbUser != null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Вы уже зарегистрированы!");
                await commandContext.RespondAsync(discordEmbed);
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

            dbContext.Users.Add(dbUser);
            await dbContext.SaveChangesAsync();

            DiscordDmChannel userChat = await member.CreateDmChannelAsync();
            DiscordEmbedBuilder discordEmbedDm = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle("Регистрация прошла успешно!");
            discordEmbedDm.AddField("Логин", $"Используй **{dbUser.Nikname}** или **{dbUser.Id}**");
            discordEmbedDm.AddField("Пароль", password);
            await userChat.SendMessageAsync(discordEmbedDm);

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Регистрация прошла успешно! Пароль для входа отправлен в личные сообщения. (≧◡≦)");
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("help")]
        [Description("Показывает подсказку по командам и модулям (в случае если модуль или команда не указаны выводит подсказку по всем командам)")]
        public async Task Help(CommandContext commandContext,
            [Description("Модуль или команда")] string moduleOrCommand = null)
        {
            string botPrefix = YukoSettings.Current.BotPrefix;
            string botDescription = YukoSettings.Current.BotDescription;
            if (moduleOrCommand != null)
            {
                if (CheckHelpCategoryCommand(moduleOrCommand))
                {
                    IEnumerable<Command> commands = commandContext.CommandsNext.RegisteredCommands.Values.Distinct()
                        .Where(x => ((x.Module as SingletonCommandModule).Instance as CommandModule).Category.HelpCommand.Equals(moduleOrCommand) &&
                            !x.IsHidden && !x.RunChecksAsync(commandContext, true).Result.Any());

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

                    Category category = GetCategoryByHelpCommand(moduleOrCommand);

                    if (commandOfDescription.Count > 0)
                    {
                        embed = new DiscordEmbedBuilder()
                        {
                            Title = $"(≧◡≦) {botPrefix}",
                            Color = DiscordColor.Orange,
                            Footer = new DiscordEmbedBuilder.EmbedFooter()
                            {
                                Text = $"v{Assembly.GetExecutingAssembly().GetName().Version}"
                            },
                            Description = botDescription
                        };

                        embed.AddField(category.Name, new string('=', category.Name.Length));
                        foreach (string[] item in commandOfDescription)
                        {
                            embed.AddField(item[0], item[1]);
                        }
                    }
                    else
                    {
                        embed = new DiscordEmbedBuilder()
                        {
                            Title = commandContext.Member.DisplayName,
                            Color = DiscordColor.Red,
                            Description = category.AccessError
                        };
                    }

                    await commandContext.RespondAsync(embed);
                }
                else
                {
                    Command command = commandContext.CommandsNext.FindCommand(moduleOrCommand, out string args);

                    if (command == null)
                        throw new CommandNotFoundException(moduleOrCommand);

                    IEnumerable<CheckBaseAttribute> failedChecks = await command.RunChecksAsync(commandContext, true);
                    if (failedChecks.Any())
                        throw new ChecksFailedException(command, commandContext, failedChecks);

                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                    {
                        Title = $"Описание команды {command.Name}",
                        Color = DiscordColor.Orange,
                        Footer = new DiscordEmbedBuilder.EmbedFooter()
                        {
                            Text = $"v{Assembly.GetExecutingAssembly().GetName().Version}"
                        },
                    };

                    StringBuilder sb = new StringBuilder();

                    bool countOverloads = command.Overloads.Count > 1;

                    for (int i = 0; i < command.Overloads.Count; i++)
                    {
                        CommandOverload commandOverload = command.Overloads[i];

                        if (countOverloads)
                        {
                            sb.AppendLine($"**__Вариант {i + 1}__**");
                        }

                        sb.AppendLine($"```\n{botPrefix} {command.Name} {string.Join(' ', commandOverload.Arguments.Select(x => $"[{x.Name}]").ToList())}```{command.Description}");
                        sb.AppendLine();

                        if (command.Aliases?.Count != 0)
                        {
                            sb.AppendLine("**Алиасы:**");
                            foreach (string alias in command.Aliases)
                            {
                                sb.Append($"{alias} ");
                            }
                            sb.AppendLine();
                            sb.AppendLine();
                        }

                        if (commandOverload.Arguments.Count != 0)
                        {
                            sb.AppendLine("**Аргументы:**");
                            foreach (CommandArgument argument in commandOverload.Arguments)
                            {
                                string defaultValue = (argument.DefaultValue != null) ? $" (По умолчанию: {argument.DefaultValue})" : string.Empty;
                                sb.AppendLine($"`{argument.Name}`: {argument.Description}{defaultValue}");
                            }
                            sb.AppendLine();
                        }
                    }

                    embed.WithDescription(sb.ToString());

                    await commandContext.RespondAsync(embed);
                }
            }
            else
            {
                IEnumerable<Command> commands = commandContext.CommandsNext.RegisteredCommands.Values.Distinct()
                    .Where(x => !x.IsHidden && !x.RunChecksAsync(commandContext, true).Result.Any());

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
                {
                    Title = $"(≧◡≦) {botPrefix}",
                    Color = DiscordColor.Orange,
                    Footer = new DiscordEmbedBuilder.EmbedFooter()
                    {
                        Text = $"v{Assembly.GetExecutingAssembly().GetName().Version}"
                    },
                    Description = botDescription
                };

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

                await commandContext.RespondAsync(embed);
            }
        }
    }
}