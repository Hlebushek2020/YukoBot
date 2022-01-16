using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace YukoBot.Commands.Formatter
{
    public class HelpFormatter : BaseHelpFormatter
    {
        protected DiscordEmbedBuilder embed;

        public HelpFormatter(CommandContext commandContext) : base(commandContext) =>
            embed = new DiscordEmbedBuilder();

        public override CommandHelpMessage Build()
        {
            embed.WithColor(DiscordColor.Orange)
                .WithFooter($"v{Assembly.GetExecutingAssembly().GetName().Version}");
            return new CommandHelpMessage(embed: embed);
        }

        public override BaseHelpFormatter WithCommand(Command command)
        {
            embed.WithTitle("Описание команды");

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

                //StringBuilder sbArgValues = new StringBuilder();

                if (commandOverload.Arguments.Count != 0)
                {
                    sb.AppendLine("**Аргументы:**");
                    foreach (CommandArgument argument in commandOverload.Arguments)
                    {
                        string defaultValue = (argument.DefaultValue != null) ? $" (По умолчанию: {argument.DefaultValue})" : string.Empty;
                        sb.AppendLine($"`{argument.Name}`: {argument.Description}{defaultValue}");
                        //foreach (System.Attribute attribute in c.CustomAttributes)
                        //{
                        //    if (attribute is ArgumentValuesAttribute)
                        //    {
                        //        sbArgValues.Append($"{c.Name}: ");
                        //        foreach (string value in ((ArgumentValuesAttribute)attribute).ArgumentValues)
                        //        {
                        //            sbArgValues.Append($"`{value}` ");
                        //        }
                        //        sbArgValues.AppendLine();
                        //    }
                        //}
                    }
                    sb.AppendLine();

                    //if (sbArgValues.Length != 0)
                    //{
                    //    sb.AppendLine("**Допустимые значения**");
                    //    sb.Append(sbArgValues);
                    //    sb.AppendLine();
                    //}
                }
            }

            embed.WithDescription(sb.ToString());

            return this;
        }

        public override BaseHelpFormatter WithSubcommands(IEnumerable<Command> subcommands)
        {
            embed.Title = "≧◡≦ | >yuko";
            embed.Description = "Бот предназначен для скачивания вложений(я) из сообщений(я). Больше информации тут: https://github.com/Hlebushek2020/YukoBot";

            Dictionary<string, List<string[]>> sortedCommand = new Dictionary<string, List<string[]>>();

            foreach (Command command in subcommands)
            {
                if (command.Module is null)
                    continue;

                if (!(command.Module is SingletonCommandModule))
                    continue;

                CommandModule yukoModule = (command.Module as SingletonCommandModule).Instance as CommandModule;

                if (string.IsNullOrEmpty(yukoModule.ModuleName))
                    continue;

                if (!sortedCommand.ContainsKey(yukoModule.ModuleName))
                    sortedCommand.Add(yukoModule.ModuleName, new List<string[]>());

                StringBuilder aliases = new StringBuilder();
                foreach (string alias in command.Aliases)
                    aliases.Append(alias).Append(", ");

                string fullNameCommand = command.Name;
                if (aliases.Length > 0)
                {
                    aliases.Remove(aliases.Length - 2, 2);
                    fullNameCommand = $"{fullNameCommand} ({aliases})";
                }
                sortedCommand[yukoModule.ModuleName].Add(new string[] { fullNameCommand, command.Description });
            }

            foreach (KeyValuePair<string, List<string[]>> commandsEntry in sortedCommand)
            {
                embed.AddField(commandsEntry.Key, new string('=', commandsEntry.Key.Length));
                foreach (string[] item in commandsEntry.Value)
                {
                    embed.AddField(item[0], item[1]);
                }
            }

            return this;
        }
    }
}