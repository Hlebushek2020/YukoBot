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
using YukoBot.Commands.Exceptions;
using YukoBot.Extensions;
using YukoBot.Models.Database;
using YukoBot.Models.Database.Entities;
using YukoBot.Services;
using YukoBot.Services.Implementation;

namespace YukoBot
{
    public class YukoBot : IYukoBot, IDisposable
    {
        #region Fields
        private readonly DiscordClient _discordClient;
        private readonly TcpListener _tcpListener;
        private readonly ILogger<YukoBot> _logger;
        private readonly IServiceProvider _services;
        private readonly IYukoSettings _yukoSettings;
        private readonly YukoDbContext _dbContext;
        private readonly IBotNotificationsService _notificationsService;
        private readonly IMessageRequestQueueService _mrqService;

        private Task _processTask;
        private CancellationTokenSource _processCts;
        private bool _isDisposed;

        private volatile bool _isOff;
        #endregion

        #region Property
        public DateTime StartDateTime { get; private set; }
        public bool IsShutdown => _processCts is { IsCancellationRequested: true };
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

            _discordClient.SessionCreated += DiscordClient_OnSessionCreated;

            _logger.LogInformation("Initializing services");

            _services = new ServiceCollection()
                .AddLogging(lb => lb.AddSerilog(dispose: true))
                .AddSingleton(_discordClient)
                .AddSingleton(_yukoSettings)
                .AddSingleton(typeof(IYukoBot), this)
                .AddDbContext<YukoDbContext>()
                .AddSingleton<IBotNotificationsService, BotNotificationsService>()
                .AddSingleton<IBotPingService, BotPingService>()
                .AddSingleton<IDeletingMessagesByEmojiService, DeletingMessagesByEmojiService>()
                .AddSingleton<IMessageRequestQueueService, MessageRequestQueueService>()
                .BuildServiceProvider();

            // Initializing services that won't be called anywhere
            _services.GetService<IBotPingService>();
            _services.GetService<IDeletingMessagesByEmojiService>();

            _dbContext = _services.GetService<YukoDbContext>();
            _notificationsService = _services.GetService<IBotNotificationsService>();
            _mrqService = _services.GetService<IMessageRequestQueueService>();

            _logger.LogInformation("Initializing commands");

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

        private async Task DiscordClient_OnSessionCreated(DiscordClient sender, SessionReadyEventArgs args)
        {
            await _discordClient.UpdateStatusAsync(
                new DiscordActivity(
                    string.Format(
                        Resources.Bot_Activity,
                        Constants.HappySmile,
                        _yukoSettings.BotPrefix),
                    ActivityType.Watching));
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            _logger.LogInformation($"Command {e.Command.Name} completed successfully");
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
                embed.WithDescription(string.Format(Resources.Bot_CommandErrored_ArgumentException, command.Name));

                _logger.LogWarning(
                    $"Error when executing the {command.Name} command. Type: ArgumentException. Message: {
                        exception.Message}");
            }
            else if (exception is CommandNotFoundException commandNotFoundEx)
            {
                embed.WithDescription(
                    string.Format(
                        Resources.Bot_CommandErrored_CommandNotFoundException,
                        commandNotFoundEx.CommandName));

                _logger.LogWarning(
                    $"Error when executing the {commandNotFoundEx.CommandName
                    } command. Type: CommandNotFoundException. Message: {exception.Message}");
            }
            else if (exception is ChecksFailedException checksFailedEx)
            {
                CommandModule yukoModule =
                    (checksFailedEx.Command.Module as SingletonCommandModule)?.Instance as CommandModule;
                embed.WithDescription(
                    !string.IsNullOrEmpty(yukoModule?.CommandAccessError)
                        ? yukoModule.CommandAccessError
                        : exception.Message);

                _logger.LogWarning(
                    $"Error when executing the {checksFailedEx.Command.Name
                    } command. Type: CommandNotFoundException. Message: {exception.Message}");
            }
            else if (exception is ShutdownBotException) { }
            else
            {
                embed.WithDescription(Resources.Bot_CommandErrored_UnknownException);

                _logger.LogError(exception, $"Error when executing the: {e.Command?.Name ?? "Unknown"}");
            }

            bool sendToCurrentChannel = true;
            if (dMember != null && command != null &&
                (command.Name.Equals("add", StringComparison.OrdinalIgnoreCase) ||
                 command.Name.Equals("start", StringComparison.OrdinalIgnoreCase) ||
                 command.Name.Equals("end", StringComparison.OrdinalIgnoreCase)))
            {
                DbGuildSettings dbGuildSettings = await _dbContext.GuildsSettings.FindAsync(context.Guild.Id);
                if (dbGuildSettings != null)
                    sendToCurrentChannel = dbGuildSettings.AddCommandResponse;
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
            if (_processCts != null && !_processTask.IsCompleted)
                throw new Exception();

            _processCts = new CancellationTokenSource();
            CancellationToken processToken = _processCts.Token;
            _processTask = Task.Run(
                async () =>
                {
                    try
                    {
                        _logger.LogInformation("Discord client connect");
                        await _discordClient.ConnectAsync();
                        StartDateTime = DateTime.Now;

                        await _notificationsService.SendReadyNotifications();

                        _tcpListener.Start();
                        _logger.LogInformation("Server listening");

                        while (!processToken.IsCancellationRequested)
                        {
                            if (_tcpListener.Pending())
                            {
                                YukoClient yukoClient =
                                    new YukoClient(
                                        _services,
                                        await _tcpListener.AcceptTcpClientAsync(processToken));
                                ThreadPool.QueueUserWorkItem(yukoClient.Process);
                            }
                            else
                            {
                                // ReSharper disable once MethodSupportsCancellation
                                await Task.Delay(200);
                            }
                        }

                        while (!_isOff)
                        {
                            // ReSharper disable once MethodSupportsCancellation
                            await Task.Delay(200);
                        }
                    }
                    catch (TaskCanceledException) { }
                },
                processToken);

            return _processTask;
        }

        public void Shutdown(string reason = null)
        {
            _logger.LogInformation("Shutdown");
            _discordClient.UpdateStatusAsync(
                new DiscordActivity(
                    Resources.Bot_Shutdown,
                    ActivityType.Custom),
                UserStatus.DoNotDisturb).Wait();

            _logger.LogInformation("Server stopping listener");
            if (_processCts != null && !_processTask.IsCompleted)
                _processCts.Cancel();
            _tcpListener?.Stop();

            _logger.LogInformation("Waiting for clients to disconnect");
            while (YukoClient.Availability)
                Task.Delay(100).Wait();

            _logger.LogInformation("Stopping the message request service");
            _mrqService.StopProcessing().Wait();

            if (!string.IsNullOrEmpty(reason))
                _notificationsService.SendShutdownNotifications(reason).Wait();

            if (_discordClient != null)
            {
                _logger.LogInformation("Disconnect discord client");
                _discordClient.DisconnectAsync().Wait();
            }

            _isOff = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed && disposing)
            {
                if (!_processCts.IsCancellationRequested)
                    Shutdown();

                _processTask?.Dispose();
                _discordClient?.Dispose();

                _isDisposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}