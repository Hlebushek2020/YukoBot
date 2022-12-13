using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using YukoBot.Commands;
using YukoBot.Interfaces;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;
using YukoBot.Models.Log;
using YukoBot.Models.Log.Providers;

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
        private static CancellationTokenSource processCts;

        private readonly TcpListener tcpListener;

        private readonly ILogger _defaultLogger;

        private readonly int messageLimit = YukoSettings.Current.DiscordMessageLimit;
        private readonly int messageLimitSleepMs = YukoSettings.Current.DiscordMessageLimitSleepMs;

        private YukoBot()
        {
            YukoLoggerFactory loggerFactory = YukoLoggerFactory.Current;
            loggerFactory.AddProvider(new DiscordClientLoggerProvider(LogLevel.Error));
            _defaultLogger = loggerFactory.CreateLogger<DefaultLoggerProvider>();

            EventId eventId = new EventId(0, "Init");
            _defaultLogger.LogInformation(eventId, "Initializing discord client");

            IReadOnlyYukoSettings settings = YukoSettings.Current;

            discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = settings.BotToken,
                TokenType = TokenType.Bot,
                LoggerFactory = loggerFactory,
                Intents = DiscordIntents.All
            });

            discordClient.Ready += DiscordClient_Ready;
            // discordClient.MessageReactionAdded += DiscordClient_MessageReactionAdded;
            discordClient.SocketErrored += DiscordClient_SocketErrored;

            CommandsNextExtension commands = discordClient.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new List<string> { settings.BotPrefix },
                EnableDefaultHelp = false
            });

            commands.RegisterCommands<OwnerCommandModule>();
            commands.RegisterCommands<AdminCommandModule>();
            commands.RegisterCommands<UserCommandModule>();
            commands.RegisterCommands<RegisteredUserCommandModule>();
            commands.RegisterCommands<ManagingСollectionsCommandModule>();

            commands.CommandErrored += Commands_CommandErrored;
            commands.CommandExecuted += Commands_CommandExecuted;

            _defaultLogger.LogInformation(eventId, "Server initialization");

            tcpListener = new TcpListener(IPAddress.Parse(settings.ServerInternalAddress), settings.ServerPort);
        }

        private async Task DiscordClient_Ready(DiscordClient sender, ReadyEventArgs e) =>
            await sender.UpdateStatusAsync(new DiscordActivity($"на тебя (≧◡≦) | {YukoSettings.Current.BotPrefix} help", ActivityType.Watching));

        private Task DiscordClient_SocketErrored(DiscordClient sender, SocketErrorEventArgs e)
        {
            _defaultLogger.LogCritical(new EventId(0, "Discord Client: Socket Errored"), e.Exception, "");
            Environment.Exit(1);
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            _defaultLogger.LogInformation(new EventId(0, $"Command: {e.Command.Name}"), "Command completed successfully");
            return Task.CompletedTask;
        }

        //private async Task DiscordClient_MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        //{
        //    if (e.Guild == null)
        //    {
        //        DiscordEmoji emoji = DiscordEmoji.FromName(sender, ":negative_squared_cross_mark:", false);
        //        if (e.Emoji.Equals(emoji))
        //        {
        //            await e.Message.DeleteAsync();
        //        }
        //    }
        //}

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
                _defaultLogger.LogWarning(new EventId(0, $"Command: {e.Command.Name}"), exception, "");
            }
            else if (exception is CommandNotFoundException commandNotFoundEx)
            {
                embed.WithDescription($"Простите, я не знаю команды {commandNotFoundEx.CommandName} (⋟﹏⋞)");
                _defaultLogger.LogWarning(new EventId(0, $"Command: {commandNotFoundEx.CommandName}"), exception, "");
            }
            else if (exception is ChecksFailedException checksFailedEx)
            {
                CommandModule yukoModule = (checksFailedEx.Command.Module as SingletonCommandModule).Instance as CommandModule;
                if (!string.IsNullOrEmpty(yukoModule.CommandAccessError))
                {
                    embed.WithDescription(yukoModule.CommandAccessError);
                }
                _defaultLogger.LogWarning(new EventId(0, $"Command: {checksFailedEx.Command.Name}"), exception, "");
            }
            else
            {
                embed.WithDescription("Простите, при выполнении команды произошла неизвестная ошибка (⋟﹏⋞), попробуйте обратиться к моему создателю");
                _defaultLogger.LogError(new EventId(0, $"Command: {e.Command?.Name ?? "Unknown"}"), exception, "");
            }

            bool sendToCurrentChannel = true;
            if (dMember != null && command != null && command.Name.Equals("add", StringComparison.OrdinalIgnoreCase))
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

        public Task RunAsync()
        {
            if (processCts == null || processTask.IsCompleted)
            {
                processCts = new CancellationTokenSource();
                CancellationToken processToken = processCts.Token;
                processTask = Task.Run(async () =>
                {
                    EventId eventId = new EventId(0, "Run");
                    _defaultLogger.LogInformation(eventId, "Discord client connect");

                    await discordClient.ConnectAsync();

                    StartDateTime = DateTime.Now;

                    tcpListener.Start();

                    _defaultLogger.LogInformation(eventId, "Server listening");

                    while (!processToken.IsCancellationRequested)
                    {
                        if (!tcpListener.Pending())
                        {
                            Thread.Sleep(100);
                            continue;
                        }

                        ThreadPool.QueueUserWorkItem(TcpClientProcessing, tcpListener.AcceptTcpClient());
                    }
                }, processToken);
            }
            return processTask;
        }

        public void Shutdown()
        {
            EventId eventId = new EventId(0, "Shutdown");
            _defaultLogger.LogInformation(eventId, "Shutdown");

            _defaultLogger.LogInformation(eventId, "Server stopping listener");
            if (processCts != null && !processTask.IsCompleted)
            {
                processCts.Cancel();
                processTask.Wait();
            }
            tcpListener?.Stop();

            _defaultLogger.LogInformation(eventId, "Waiting for clients to disconnect");
            while (countClient > 0)
            {
                Thread.Sleep(100);
            }

            if (discordClient != null)
            {
                _defaultLogger.LogInformation(eventId, "Disconnect discord client");
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
                Shutdown();
                processTask?.Dispose();
                discordClient?.Dispose();
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