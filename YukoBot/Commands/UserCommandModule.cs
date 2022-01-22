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
        public UserCommandModule() : base(User) { }

        [Command("register")]
        [Aliases("reg")]
        [Description("Регистрация")]
        public async Task Register(CommandContext commandContext)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithTitle($"{commandContext.Member.DisplayName}");

            DiscordMember user = commandContext.Member;

            YukoDbContext database = new YukoDbContext();
            DbUser dbUser = database.Users.Find(user.Id);
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

            database.Users.Add(dbUser);
            await database.SaveChangesAsync();

            DiscordDmChannel userChat = await user.CreateDmChannelAsync();
            DiscordEmbedBuilder discordEmbedDm = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle("Регистрация прошла успешно!");
            discordEmbedDm.AddField("Логин", $"Используй **{dbUser.Nikname}** или **{dbUser.Id}**");
            discordEmbedDm.AddField("Пароль", password);
            await userChat.SendMessageAsync(discordEmbedDm);

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Регистрация прошла успешно! Пароль для входа отправлен в личные сообщения. ≧◡≦");
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("ban-reason")]
        [Aliases("reason")]
        [Description("Причина бана на текущем сервере")]
        public async Task BanReason(CommandContext commandContext)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                 .WithTitle($"{commandContext.Member.DisplayName}");

            YukoDbContext database = new YukoDbContext();
            DbUser dbUser = database.Users.Find(commandContext.Member.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Вы не зарегистрированы!");
                await commandContext.RespondAsync(discordEmbed);
                return;
            }

            List<DbBan> currentBans = database.Bans.Where(x => x.ServerId == commandContext.Guild.Id && x.UserId == commandContext.Member.Id).ToList();
            if (currentBans.Count > 0)
            {
                discordEmbed.WithColor(DiscordColor.Orange);

                DbBan ban = currentBans[0];
                if (string.IsNullOrEmpty(ban.Reason))
                {
                    discordEmbed.WithDescription("К сожалению причина бана не была указана.");
                }
                else
                {
                    discordEmbed.WithDescription(ban.Reason);
                }
                await commandContext.RespondAsync(discordEmbed);
                return;
            }

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Вы не забанены. ≧◡≦");
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("password-reset")]
        [Aliases("password")]
        [Description("Сброс пароля")]
        public async Task PasswordReset(CommandContext commandContext)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                 .WithTitle($"{commandContext.Member.DisplayName}");

            YukoDbContext database = new YukoDbContext();
            DbUser dbUser = database.Users.Find(commandContext.Member.Id);
            if (dbUser == null)
            {
                discordEmbed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Вы не зарегистрированы!");
                await commandContext.RespondAsync(discordEmbed);
                return;
            }

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

            await database.SaveChangesAsync();

            DiscordDmChannel userChat = await commandContext.Member.CreateDmChannelAsync();
            DiscordEmbedBuilder discordEmbedDm = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle("Пароль сменен!");
            discordEmbedDm.AddField("Новый пароль", password);
            await userChat.SendMessageAsync(discordEmbedDm);

            discordEmbed
                .WithColor(DiscordColor.Orange)
                .WithDescription("Пароль сменен! Новый пароль отправлен в личные сообщения. ≧◡≦");
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("settings")]
        [Description("Данные для подключения.")]
        public async Task GetClientSettings(CommandContext commandContext)
        {
            YukoSettings settings = YukoSettings.Current;

            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle($"{commandContext.Member.DisplayName}");
            discordEmbed.AddField("Хост", settings.ServerAddress);
            discordEmbed.AddField("Порт", settings.ServerPort.ToString());
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("app")]
        [Description("Ссылка на скачивание актуальной версии клиента.")]
        public async Task GetClientApp(CommandContext commandContext)
        {
            DiscordEmbedBuilder discordEmbed = new DiscordEmbedBuilder()
                .WithColor(DiscordColor.Orange)
                .WithTitle($"{commandContext.Member.DisplayName}")
                .WithDescription(YukoSettings.Current.ClientActualApp);
            await commandContext.RespondAsync(discordEmbed);
        }

        [Command("help")]
        [Description("Показывает подсказку по командам и модулям (в случае если модуль или команда не указаны выводит подсказку по всем командам)")]
        public async Task Help(CommandContext commandContext,
            [Description("Модуль или команда")] string moduleOrCommand = null)
        {
            string botPrefix = YukoSettings.Current.BotPrefix;
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
                            Title = $"≧◡≦ | {botPrefix}",
                            Color = DiscordColor.Orange,
                            Footer = new DiscordEmbedBuilder.EmbedFooter()
                            {
                                Text = $"v{Assembly.GetExecutingAssembly().GetName().Version}"
                            },
                            Description = "Бот предназначен для скачивания вложений(я) из сообщений(я). Больше информации тут: https://github.com/Hlebushek2020/YukoBot"
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
                        throw new CommandNotFoundException(command.Name);

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

                        sb.AppendLine($"```\n>yuko {command.Name} {string.Join(' ', commandOverload.Arguments.Select(x => $"[{ x.Name}]").ToList())}```{command.Description}");
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
                    Title = $"≧◡≦ | {botPrefix}",
                    Color = DiscordColor.Orange,
                    Footer = new DiscordEmbedBuilder.EmbedFooter()
                    {
                        Text = $"v{Assembly.GetExecutingAssembly().GetName().Version}"
                    },
                    Description = "Бот предназначен для скачивания вложений(я) из сообщений(я). Больше информации тут: https://github.com/Hlebushek2020/YukoBot"
                };

                foreach (Category mInfo in Categories)
                {
                    string categoryName = mInfo.Name;
                    string fieldName = $"{categoryName} (Модуль: {mInfo.HelpCommand})";
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