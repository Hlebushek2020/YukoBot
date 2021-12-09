using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Threading;

namespace YukoClient.Models.Progress
{
    public class ExportScripts : Base
    {
        private readonly ICollection<Script> scripts;
        private readonly string fileName;
        private readonly ulong serverId;

        public ExportScripts(ICollection<Script> scripts, ulong serverId, string fileName)
        {
            this.scripts = scripts;
            this.fileName = fileName;
            this.serverId = serverId;
        }

        public override void Run(Dispatcher dispatcher)
        {
            dispatcher.Invoke(() => State = "Подготовка к экспорту правил");
            using (FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                using (BinaryWriter binaryWriter = new BinaryWriter(fileStream, Encoding.UTF8))
                {
                    binaryWriter.Write(App.BinaryFileVersion);
                    binaryWriter.Write(serverId);
                    binaryWriter.Write(scripts.Count);
                    dispatcher.Invoke(() => MaxValue = scripts.Count);
                    foreach (Script scriptItem in scripts)
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