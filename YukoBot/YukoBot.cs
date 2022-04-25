using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using YukoBot.Commands;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;
using YukoBot.Models.Log;

namespace YukoBot
{
    public partial class YukoBot : IDisposable
    {
        #region Instance
        private static YukoBot yukoBot;

        public static YukoBot Current
        {
            get
            {
                if (yukoBot == null)
                {
                    yukoBot = new YukoBot();
                }
                return yukoBot;
            }
        }
        #endregion

        public bool IsDisposed { get; private set; } = false;
        public DateTime StartDateTime { get; private set; }

        private readonly DiscordClient discordClient;

        private Task processTask;
        private volatile bool isRuning = false;

        private readonly TcpListener tcpListener;
        private readonly ServerLogger serverLogger;
        private readonly CommandLogger commandLoger;

        private readonly int messageLimit = YukoSettings.Current.DiscordMessageLimit;
        private readonly int messageLimitSleepMs = YukoSettings.Current.DiscordMessageLimitSleepMs;

        private YukoBot()
        {
            YukoLoggerFactory loggerFactory = new YukoLoggerFactory(LogLevel.Error);
            serverLogger = loggerFactory.CreateLogger<ServerLogger>();
            commandLoger = loggerFactory.CreateLogger<CommandLogger>();

            serverLogger.Log(LogLevel.Information, "Initialization Discord Api");

            YukoSettings settings = YukoSettings.Current;

            discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = settings.BotToken,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Error,
                LoggerFactory = loggerFactory
            });

            discordClient.Ready += DiscordClient_Ready;
            discordClient.MessageReactionAdded += DiscordClient_MessageReactionAdded;
            discordClient.SocketErrored += DiscordClient_SocketErrored;

            CommandsNextExtension commands = discordClient.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { settings.BotPrefix },
                EnableDefaultHelp = false
            });

            commands.RegisterCommands<OwnerCommandModule>();
            commands.RegisterCommands<AdminCommandModule>();
            commands.RegisterCommands<UserCommandModule>();
            commands.RegisterCommands<RegisteredUserCommandModule>();
            commands.RegisterCommands<ManagingСollectionsCommandModule>();

            commands.CommandErrored += Commands_CommandErrored;
            commands.CommandExecuted += Commands_CommandExecuted;

            serverLogger.Log(LogLevel.Information, "Initialization Server");

            tcpListener = new TcpListener(IPAddress.Parse(settings.ServerInternalAddress), settings.ServerPort);
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            DiscordUser dUser = e.Context.User;
            commandLoger.Log(dUser, "SUCCESS", null, e.Command.Name);
            return Task.CompletedTask;
        }

        private async Task DiscordClient_MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            if (e.Guild == null)
            {
                DiscordEmoji emoji = DiscordEmoji.FromName(sender, ":negative_squared_cross_mark:", false);
                if (e.Emoji.Equals(emoji))
                {
                    await e.Message.DeleteAsync();
                }
            }
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            CommandContext context = e.Context;
            Exception exception = e.Exception;
            Command command = e.Command;
            DiscordMember dMember = context.Member;
            DiscordUser dUser = context.User;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Title = dMember?.DisplayName,
                Color = DiscordColor.Red
            };

            if (exception is ArgumentException)
            {
                embed.WithDescription($"Простите, в команде {command.Name} ошибка (⋟﹏⋞)");
                commandLoger.Log(dUser, "ERROR", exception, command.Name);
            }
            else if (exception is CommandNotFoundException commandNotFoundEx)
            {
                embed.WithDescription($"Простите, я не знаю команды {commandNotFoundEx.CommandName} (⋟﹏⋞)");
                commandLoger.Log(dUser, "ERROR", exception, commandNotFoundEx.CommandName);
            }
            else if (exception is ChecksFailedException)
            {
                CommandModule yukoModule = (command.Module as SingletonCommandModule).Instance as CommandModule;
                if (!string.IsNullOrEmpty(yukoModule.CommandAccessError))
                {
                    embed.WithDescription(yukoModule.CommandAccessError);
                }
                commandLoger.Log(dUser, "ERROR", exception, command.Name);
            }
            else
            {
                embed.WithDescription("Простите, при выполнении команды произошла неизвестная ошибка (⋟﹏⋞), попробуйте обратиться к моему создателю");
                commandLoger.Log(dUser, "ERROR", exception, command?.Name ?? "Unknown", true);
            }

            if (dMember != null)
            {
                bool sendToCurrentChannel = true;
                if (command != null && command.Name.Equals("add", StringComparison.OrdinalIgnoreCase))
                {
                    YukoDbContext dbContext = new YukoDbContext();
                    DbGuildSettings dbGuildSettings = dbContext.GuildsSettings.Find(context.Guild.Id);
                    if (dbGuildSettings != null)
                    {
                        sendToCurrentChannel = dbGuildSettings.AddCommandResponse;
                    }
                }

                if (sendToCurrentChannel)
                {
                    await context.RespondAsync(embed);
                }
                else
                {
                    DiscordDmChannel discordDmChannel = await dMember.CreateDmChannelAsync();
                    await discordDmChannel.SendMessageAsync(embed);
                }
            }
        }

        private async Task DiscordClient_Ready(DiscordClient sender, ReadyEventArgs e) =>
            await sender.UpdateStatusAsync(new DiscordActivity($"на тебя (≧◡≦) | {YukoSettings.Current.BotPrefix} help", ActivityType.Watching));

        private Task DiscordClient_SocketErrored(DiscordClient sender, SocketErrorEventArgs e)
        {
            serverLogger.Log(LogLevel.Critical, "Discord Api", e);
            Environment.Exit(1);
            return Task.CompletedTask;
        }

        public Task RunAsync()
        {
            processTask = Process();
            return processTask;
        }

        private async Task Process()
        {
            if (isRuning)
            {
                return;
            }

            isRuning = true;

            serverLogger.Log(LogLevel.Information, "Discord Api Authorization");

            await discordClient.ConnectAsync();

            StartDateTime = DateTime.Now;

            tcpListener.Start();

            serverLogger.Log(LogLevel.Information, "Server Listening");

            while (isRuning)
            {
                if (!tcpListener.Pending())
                {
                    Thread.Sleep(100);
                    continue;
                }

                ThreadPool.QueueUserWorkItem(TcpClientProcessing, tcpListener.AcceptTcpClient());
            }
        }

        public void Shutdown()
        {
            serverLogger.Log(LogLevel.Information, "Shutdown");

            isRuning = false;

            serverLogger.Log(LogLevel.Information, "Server stopping listener");

            if (tcpListener != null)
            {
                tcpListener.Stop();
            }

            if (processTask != null)
            {
                processTask.Wait();
            }

            serverLogger.Log(LogLevel.Information, "Discord Api Disconnect");

            if (discordClient != null)
            {
                discordClient.DisconnectAsync().Wait();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }

            if (disposing)
            {
                if (isRuning)
                {
                    Shutdown();
                }

                if (processTask != null)
                {
                    processTask.Dispose();
                }

                if (discordClient != null)
                {
                    discordClient.Dispose();
                }
            }

            IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}