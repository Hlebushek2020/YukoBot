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

        private readonly int messageLimit = YukoSettings.Current.DiscordMessageLimit;
        private readonly int messageLimitSleepMs = YukoSettings.Current.DiscordMessageLimitSleepMs;

        private YukoBot()
        {
            Logger.WriteServerLog("Initialization Discord Api ...");

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
            commands.CommandExecuted += Commands_CommandExecuted;

            Logger.WriteServerLog("Initialization Server ...");
            tcpListener = new TcpListener(IPAddress.Parse(settings.ServerInternalAddress), settings.ServerPort);
        }

        private Task Commands_CommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
        {
            DiscordUser dUser = e.Context.User;
            Logger.WriteCommandLog($"{dUser.Username}#{dUser.Discriminator}; {dUser.Id}; SUCCESS; {e.Command.Name}");
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
                Color = DiscordColor.Red,
                Title = dMember?.DisplayName
            };

            if (exception is ArgumentException)
            {
                embed.WithDescription($"Простите, в команде {command.Name} ошибка (\\*^.^*)");
                Logger.WriteCommandLog($"{dUser.Username}#{dUser.Discriminator}; {dUser.Id}; ERROR; ArgumentException; {command.Name}");
            }
            else if (exception is CommandNotFoundException commandNotFoundEx)
            {
                embed.WithDescription($"Простите, я не знаю команды {commandNotFoundEx.CommandName} (\\*^.^*)");
                Logger.WriteCommandLog($"{dUser.Username}#{dUser.Discriminator}; {dUser.Id}; ERROR; CommandNotFoundException; {commandNotFoundEx.CommandName}");
            }
            else if (exception is ChecksFailedException)
            {
                CommandModule yukoModule = (command.Module as SingletonCommandModule).Instance as CommandModule;
                if (!string.IsNullOrEmpty(yukoModule.CommandAccessError))
                {
                    embed.WithDescription(yukoModule.CommandAccessError);
                }
                Logger.WriteCommandLog($"{dUser.Username}#{dUser.Discriminator}; {dUser.Id}; ERROR; ChecksFailedException; {command.Name}");
            }
            else
            {
                embed.WithTitle("ERROR")
                    .WithColor(DiscordColor.Red)
                    .AddField("Exception Message", exception.Message)
                    .AddField("Exception Type", exception.GetType().Name)
                    .AddField("Command", command?.Name ?? "Unknown");
                Logger.WriteCommandLog($"{dUser.Username}#{dUser.Discriminator}; {dUser.Id}; ERROR; {exception.GetType().Name}; {command?.Name ?? "Unknown"}");
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
            Logger.WriteServerLog($"[CRIT ERROR] {e.Exception.Message}");
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

            Logger.WriteServerLog("[Discord Api] Authorization ...");
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
            Logger.WriteServerLog("Shutdown ...");

            isRuning = false;

            Logger.WriteServerLog("[Server] Stopping the listener ...");
            if (tcpListener != null)
            {
                tcpListener.Stop();
            }

            if (processTask != null)
            {
                processTask.Wait();
            }

            Logger.WriteServerLog("[Discord Api] Disconnect ...");
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