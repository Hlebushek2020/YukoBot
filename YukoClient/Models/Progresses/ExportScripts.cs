using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows.Threading;
using YukoClientBase.Models.Progresses;

namespace YukoClient.Models.Progresses
{
    public class ExportScripts : BaseProgressModel
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

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            dispatcher.Invoke(() => State = "Подготовка к экспорту правил");
            using (FileStream fileStream = new FileStream(_fileName, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                {
                    binaryWriter.Write(App.BinaryFileVersion);
                    binaryWriter.Write(_serverId);
                    binaryWriter.Write(_scripts.Count);
                    dispatcher.Invoke(() => MaxValue = _scripts.Count);
                    foreach (Script scriptItem in _scripts)
                    {
                        dispatcher.Invoke(() => State = $"Запись {Value + 1}/{MaxValue}");
                        binaryWriter.Write(scriptItem.Channel.Id);
                        binaryWriter.Write(scriptItem.Channel.Name);
                        binaryWriter.Write((int)scriptItem.Mode.Mode);
                        binaryWriter.Write(scriptItem.MessageId);
                        binaryWriter.Write(scriptItem.Count);
                        dispatcher.Invoke(() => Value++);
                    }
                }
            }
        }
    }
}