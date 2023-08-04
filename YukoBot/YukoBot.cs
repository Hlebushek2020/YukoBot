using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Entities;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using YukoBot.Commands;
using YukoBot.Extensions;
using YukoBot.Interfaces;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;
using YukoBot.Services;
using YukoBot.Services.Implementation;

namespace YukoBot
{
    public class YukoBot : IYukoBot, IDisposable
    {
        public DateTime StartDateTime { get; private set; }

        #region Fields
        private readonly DiscordClient _discordClient;
        private readonly TcpListener _tcpListener;
        private readonly ILogger<YukoBot> _logger;
        private readonly IServiceProvider _services;
        private readonly IYukoSettings _yukoSettings;

        private Task _processTask;
        private CancellationTokenSource _processCts;
        #endregion

        public YukoBot(IYukoSettings yukoSettings)
        {
            _yukoSettings = yukoSettings;

            ILoggerFactory loggerFactory = new LoggerFactory().AddSerilog(dispose: true);
            _logger = loggerFactory.CreateLogger<YukoBot>();

            _logger.LogInformation("Initializing discord client");

            _discordClient = new DiscordClient(
                new DiscordConfiguration
                {
                    Token = _yukoSettings.BotToken,
                    TokenType = TokenType.Bot,
                    LoggerFactory = loggerFactory,
                    Intents = DiscordIntents.All
                });

            _discordClient.Ready += DiscordClient_Ready;

            _services = new ServiceCollection()
                .AddLogging(lb => lb.AddSerilog(dispose: true))
                .AddSingleton(_discordClient)
                .AddSingleton(_yukoSettings)
                .AddSingleton(typeof(IYukoBot), this)
                .AddSingleton<IBotPingService, BotPingService>()
                .AddSingleton<IDeletingMessagesByEmojiService, DeletingMessagesByEmojiService>()
                .BuildServiceProvider();

            // Initializing services that won't be called anywhere
            _services.GetService<IBotPingService>();
            _services.GetService<IDeletingMessagesByEmojiService>();

            CommandsNextExtension commands = _discordClient.UseCommandsNext(
                new CommandsNextConfiguration
                {
                    StringPrefixes = new List<string> { yukoSettings.BotPrefix },
                    EnableDefaultHelp = false,
                    Services = _services
                });

            commands.CommandErrored += Commands_CommandErrored;
            commands.CommandExecuted += Commands_CommandExecuted;

            commands.RegisterCommands<OwnerCommandModule>();
            commands.RegisterCommands<AdminCommandModule>();
            commands.RegisterCommands<UserCommandModule>();
            commands.RegisterCommands<RegisteredUserCommandModule>();
            commands.RegisterCommands<ManagingСollectionsCommandModule>();

            _logger.LogInformation("Server initialization");
            _tcpListener = new TcpListener(
                IPAddress.Parse(_yukoSettings.ServerInternalAddress),
                _yukoSettings.ServerPort);
        }

        private async Task DiscordClient_Ready(DiscordClient sender, ReadyEventArgs e) =>
            await sender.UpdateStatusAsync(
                new DiscordActivity(
                    $"на тебя {Constants.HappySmile} | {_yukoSettings.BotPrefix} help",
                    ActivityType.Watching));

        /*
        private Task DiscordClient_SocketErrored(DiscordClient sender, SocketErrorEventArgs e)
        {
            _defaultLogger.LogCritical(new EventId(0, "Discord Client: Socket Errored"), e.Exception, "");
            Environment.Exit(1);
            return Task.CompletedTask;
        }
        */

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            _defaultLogger.LogInformation(
                new EventId(0, $"Command: {e.Command.Name}"),
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
                _processTask = Task.Run(
                    async () =>
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
                    },
                    processToken);
            }
            return _processTask;
        }

        public void Shutdown(string reason = null)
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