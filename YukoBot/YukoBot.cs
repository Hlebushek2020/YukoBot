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
using YukoBot.Extensions;
using YukoBot.Interfaces;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;
using YukoBot.Models.Log;
using YukoBot.Models.Log.Providers;
using YukoBot.Modules;
using YukoBot.Settings;

namespace YukoBot
{
    public class YukoBot : IDisposable
    {
        #region Instance
        private static YukoBot _yukoBot;

        public static YukoBot Current
        {
            get
            {
                if (_yukoBot == null)
                {
                    _yukoBot = new YukoBot();
                }
                return _yukoBot;
            }
        }
        #endregion

        public bool IsDisposed { get; private set; } = false;
        public DateTime StartDateTime { get; private set; }

        private Task _processTask;
        private static CancellationTokenSource _processCts;

        private readonly BotPingModule _botPingModule = new BotPingModule();
        private readonly DeletingMessagesByEmojiModule _deletingMessagesByEmojiModule =
            new DeletingMessagesByEmojiModule();

        private readonly DiscordClient _discordClient;
        private readonly TcpListener _tcpListener;
        private readonly ILogger _defaultLogger;

        private YukoBot()
        {
            IReadOnlyYukoSettings settings = YukoSettings.Current;

            YukoLoggerFactory loggerFactory = YukoLoggerFactory.Current;
            loggerFactory.AddProvider(new DiscordClientLoggerProvider(settings.DiscordApiLogLevel));
            _defaultLogger = loggerFactory.CreateLogger<DefaultLoggerProvider>();

            EventId eventId = new EventId(0, "Init");
            _defaultLogger.LogInformation(eventId, "Initializing discord client");

            _discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = settings.BotToken,
                TokenType = TokenType.Bot,
                LoggerFactory = loggerFactory,
                Intents = DiscordIntents.All
            });

            _discordClient.Ready += DiscordClient_Ready;
            _discordClient.SocketErrored += DiscordClient_SocketErrored;
            _discordClient.MessageCreated += _botPingModule.Handler;
            _discordClient.MessageReactionAdded += _deletingMessagesByEmojiModule.Handler;

            CommandsNextExtension commands = _discordClient.UseCommandsNext(new CommandsNextConfiguration
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

            _tcpListener = new TcpListener(IPAddress.Parse(settings.ServerInternalAddress), settings.ServerPort);
        }

        private async Task DiscordClient_Ready(DiscordClient sender, ReadyEventArgs e) =>
            await sender.UpdateStatusAsync(new DiscordActivity(
                $"на тебя {Constants.HappySmile} | {YukoSettings.Current.BotPrefix} help", ActivityType.Watching));

        private Task DiscordClient_SocketErrored(DiscordClient sender, SocketErrorEventArgs e)
        {
            _defaultLogger.LogCritical(new EventId(0, "Discord Client: Socket Errored"), e.Exception, "");
            Environment.Exit(1);
            return Task.CompletedTask;
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            _defaultLogger.LogInformation(new EventId(0, $"Command: {e.Command.Name}"),
                "Command completed successfully");
            return Task.CompletedTask;
        }

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            CommandContext context = e.Context;
            Exception exception = e.Exception;
            Command command = e.Command;
            DiscordMember dMember = context.Member;
            DiscordUser dUser = context.User;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
                .WithSadTitle(dMember != null ? dMember.DisplayName : dUser.Username)
                .WithColor(Constants.ErrorColor);

            if (exception is ArgumentException)
            {
                embed.WithDescription($"Простите, в команде `{command.Name}` ошибка!");
                _defaultLogger.LogWarning(new EventId(0, $"Command: {e.Command.Name}"), exception, "");
            }
            else if (exception is CommandNotFoundException commandNotFoundEx)
            {
                embed.WithDescription($"Простите, я не знаю команды `{commandNotFoundEx.CommandName}`!");
                _defaultLogger.LogWarning(new EventId(0, $"Command: {commandNotFoundEx.CommandName}"), exception, "");
            }
            else if (exception is ChecksFailedException checksFailedEx)
            {
                CommandModule yukoModule =
                    (checksFailedEx.Command.Module as SingletonCommandModule).Instance as CommandModule;
                if (!string.IsNullOrEmpty(yukoModule.CommandAccessError))
                {
                    embed.WithDescription(yukoModule.CommandAccessError);
                }
                _defaultLogger.LogWarning(new EventId(0, $"Command: {checksFailedEx.Command.Name}"), exception, "");
            }
            else
            {
                embed.WithDescription(
                    "Простите, при выполнении команды произошла неизвестная ошибка, попробуйте обратиться к моему создателю!");
                _defaultLogger.LogError(new EventId(0, $"Command: {e.Command?.Name ?? "Unknown"}"), exception, "");
            }

            bool sendToCurrentChannel = true;
            if (dMember != null && command != null &&
                (command.Name.Equals("add", StringComparison.OrdinalIgnoreCase) ||
                 command.Name.Equals("start", StringComparison.OrdinalIgnoreCase) ||
                 command.Name.Equals("end", StringComparison.OrdinalIgnoreCase)))
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
            if (_processCts == null || _processTask.IsCompleted)
            {
                _processCts = new CancellationTokenSource();
                CancellationToken processToken = _processCts.Token;
                _processTask = Task.Run(async () =>
                {
                    EventId eventId = new EventId(0, "Run");
                    _defaultLogger.LogInformation(eventId, "Discord client connect");

                    await _discordClient.ConnectAsync();

                    StartDateTime = DateTime.Now;

                    _tcpListener.Start();

                    _defaultLogger.LogInformation(eventId, "Server listening");

                    while (!processToken.IsCancellationRequested)
                    {
                        if (_tcpListener.Pending())
                        {
                            YukoClient yukoClient =
                                new YukoClient(_discordClient, await _tcpListener.AcceptTcpClientAsync());
                            ThreadPool.QueueUserWorkItem(yukoClient.Process);
                        }
                        else
                        {
                            await Task.Delay(200);
                        }
                    }
                }, processToken);
            }
            return _processTask;
        }

        public void Shutdown()
        {
            EventId eventId = new EventId(0, "Shutdown");
            _defaultLogger.LogInformation(eventId, "Shutdown");

            _defaultLogger.LogInformation(eventId, "Server stopping listener");
            if (_processCts != null && !_processTask.IsCompleted)
            {
                _processCts.Cancel();
                _processTask.Wait();
            }
            _tcpListener?.Stop();

            _defaultLogger.LogInformation(eventId, "Waiting for clients to disconnect");
            while (YukoClient.Availability)
            {
                Task.Delay(100);
            }

            if (_discordClient != null)
            {
                _defaultLogger.LogInformation(eventId, "Disconnect discord client");
                _discordClient.DisconnectAsync().Wait();
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                if (!_processCts.IsCancellationRequested)
                {
                    Shutdown();
                }

                _processTask?.Dispose();
                _discordClient?.Dispose();

                IsDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}