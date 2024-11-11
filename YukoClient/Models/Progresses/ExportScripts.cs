using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using YukoClientBase.Args;

namespace YukoClient.Models.Progresses
{
    public class ExportScripts
    {
        private readonly ICollection<Script> _scripts;
        private readonly string _fileName;
        private readonly ulong _serverId;

        public ExportScripts(ICollection<Script> scripts, ulong serverId, string fileName)
        {
            _scripts = scripts;
            _fileName = fileName;
            _serverId = serverId;
        }

        public Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            progress.Report(new ProgressReportArgs { Text = "Подготовка к экспорту правил" });
            using (FileStream fileStream = new FileStream(_fileName, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                {
                    binaryWriter.Write(App.BinaryFileVersion);
                    binaryWriter.Write(_serverId);
                    binaryWriter.Write(_scripts.Count);

                    progress.Report(new ProgressReportArgs
                    {
                        IsIndeterminate = false, Maximum = _scripts.Count, Minimum = 0, Value = 0
                    });

                    int counter = 0;

                    foreach (Script scriptItem in _scripts)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        counter++;

                        progress.Report(new ProgressReportArgs { Text = $"Запись {counter}/{_scripts.Count}" });

                        binaryWriter.Write(scriptItem.Channel.Id);
                        binaryWriter.Write(scriptItem.Channel.Name);
                        binaryWriter.Write((int)scriptItem.Mode.Mode);
                        binaryWriter.Write(scriptItem.MessageId);
                        binaryWriter.Write(scriptItem.Count);

                        progress.Report(new ProgressReportArgs { Value = counter });
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}