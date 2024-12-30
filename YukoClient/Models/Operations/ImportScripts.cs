using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YukoClient.Enums;
using YukoClientBase.Args;
using YukoClientBase.Models.Operations;

namespace YukoClient.Models.Operations
{
    public class ImportScripts : IOperation
    {
        private readonly SynchronizationContext _synchronizationContext;
        private readonly ICollection<Script> _scripts;
        private readonly string _fileName;
        private readonly ulong _serverId;

        public ImportScripts(ICollection<Script> scripts, ulong serverId, string fileName)
        {
            _scripts = scripts;
            _fileName = fileName;
            _serverId = serverId;
            _synchronizationContext = SynchronizationContext.Current;
        }

        public Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken) =>
            Task.Run(() => Operation(progress, cancellationToken), cancellationToken);

        private void Operation(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            progress.Report(new ProgressReportArgs { Text = "Подготовка к импорту правил" });
            using (FileStream fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream, Encoding.UTF8))
                {
                    progress.Report(new ProgressReportArgs { Text = "Проверка версии" });

                    int version = binaryReader.ReadInt32();
                    if (App.BinaryFileVersion != version)
                        throw new Exception(
                            "Невозможно открыть файл т.к. его версия не поддерживается текущей версией программы.");

                    progress.Report(new ProgressReportArgs { Text = "Проверка сервера" });

                    ulong serverId = binaryReader.ReadUInt64();
                    if (_serverId.Equals(serverId))
                        throw new Exception("Выбран некорректный сервер!");

                    int count = binaryReader.ReadInt32();

                    progress.Report(new ProgressReportArgs { Maximum = count, Minimum = 0, Value = 0 });

                    for (int numScript = 1; numScript <= count; numScript++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        progress.Report(new ProgressReportArgs { Text = $"Чтение {numScript}/{count}" });

                        ulong channelId = binaryReader.ReadUInt64();
                        string channelName = binaryReader.ReadString();
                        ScriptMode scriptMode = (ScriptMode)binaryReader.ReadInt32();
                        ulong messageId = binaryReader.ReadUInt64();
                        int messageCount = binaryReader.ReadInt32();

                        progress.Report(new ProgressReportArgs { Text = $"Добавление {numScript}/{count}" });

                        _synchronizationContext.Send(state =>
                        {
                            _scripts.Add(new Script
                            {
                                Channel = new Channel(channelId, channelName),
                                Mode = new DisplayScriptMode(scriptMode),
                                MessageId = messageId,
                                Count = messageCount
                            });
                        }, null);

                        progress.Report(new ProgressReportArgs { Value = numScript });
                    }
                }
            }
        }
    }
}