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

        private readonly int messageLimit = YukoSettings.Current.DiscordMessageLimit;
        private readonly int messageLimitSleepMs = YukoSettings.Current.DiscordMessageLimitSleepMs;

        private YukoBot()
        {
            Console.WriteLine("Initialization Discord Api ...");

            YukoSettings settings = YukoSettings.Current;

            discordClient = new DiscordClient(new DiscordConfiguration
            {
                Token = settings.BotToken,
                TokenType = TokenType.Bot,
                MinimumLogLevel = LogLevel.Error
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

            Console.WriteLine("Initialization Server ...");
            tcpListener = new TcpListener(IPAddress.Parse(settings.ServerInternalAddress), settings.ServerPort);
        }

        private Task DiscordClient_MessageReactionAdded(DiscordClient sender, MessageReactionAddEventArgs e)
        {
            if (e.Channel != null && !e.Channel.IsPrivate)
            {
                return Task.CompletedTask;
            }
            if ((e.Message.Author != null) && (e.Message.Author.Id != sender.CurrentUser.Id))
            {
                return Task.CompletedTask;
            }
            DiscordEmoji emoji = DiscordEmoji.FromName(sender, ":negative_squared_cross_mark:", false);
            if (!e.Emoji.Equals(emoji))
            {
                return Task.CompletedTask;
            }
            return e.Message.DeleteAsync();
        }

        ~YukoBot() => Dispose(false);

        private async Task Commands_CommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
        {
            CommandContext context = e.Context;
            Exception exception = e.Exception;
            Command command = e.Command;

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            {
                Title = e.Context.Member.DisplayName,
                Color = DiscordColor.Red
            };

            if (exception is ArgumentException)
            {
                embed.WithDescription($"Простите, в команде {command.Name} ошибка (\\*^.^*)");
            }
            else if (exception is CommandNotFoundException commandNotFoundEx)
            {
                embed.WithDescription($"Простите, я не знаю команды {commandNotFoundEx.CommandName} (\\*^.^*)");
            }
            else if (exception is ChecksFailedException)
            {
                CommandModule yukoModule = (command.Module as SingletonCommandModule).Instance as CommandModule;
                if (!string.IsNullOrEmpty(yukoModule.CommandAccessError))
                {
                    embed.WithDescription(yukoModule.CommandAccessError);
                }
            }
            else
            {
                embed.WithTitle("ERROR")
                    .WithColor(DiscordColor.Red)
                    .AddField("Exception Message", exception.Message)
                    .AddField("Exception Type", exception.GetType().Name)
                    .AddField("Command", command?.Name ?? "Unknown");
            }

            bool sendToCurrentChannel = true;
            if (command.Name.Equals("add", StringComparison.CurrentCultureIgnoreCase))
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
                DiscordDmChannel discordDmChannel = await context.Member.CreateDmChannelAsync();
                await discordDmChannel.SendMessageAsync(embed);
            }
        }

        private Task DiscordClient_Ready(DiscordClient sender, ReadyEventArgs e) =>
            sender.UpdateStatusAsync(new DiscordActivity($"на тебя (≧◡≦) | {YukoSettings.Current.BotPrefix} help", ActivityType.Watching));

        private Task DiscordClient_SocketErrored(DiscordClient sender, SocketErrorEventArgs e)
        {
            Console.WriteLine($"[CRIT ERROR] {e.Exception.Message}");
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

            Console.WriteLine("[Discord Api] Authorization ...");
            await discordClient.ConnectAsync();

            StartDateTime = DateTime.Now;

            tcpListener.Start();

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
            Console.WriteLine("Shutdown ...");

            isRuning = false;

            Console.WriteLine("[Server] Stopping the listener ...");
            if (tcpListener != null)
            {
                tcpListener.Stop();
            }

            if (processTask != null)
            {
                processTask.Wait();
            }

            Console.WriteLine("[Discord Api] Disconnect ...");
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