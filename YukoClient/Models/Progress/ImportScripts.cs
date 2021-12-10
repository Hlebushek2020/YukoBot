using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace YukoClient.Models.Progress
{
    public class ImportScripts : Base
    {
        private readonly ICollection<Script> scripts;
        private readonly string fileName;
        private readonly ulong serverId;

        public ImportScripts(ICollection<Script> scripts, ulong serverId, string fileName)
        {
            this.scripts = scripts;
            this.fileName = fileName;
            this.serverId = serverId;
        }

        public override void Run(Dispatcher dispatcher)
        {
            dispatcher.Invoke(() => State = "Подготовка к импорту правил");
            using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream, Encoding.UTF8))
                {
                    dispatcher.Invoke(() => State = "Проверка версии");
                    int version = binaryReader.ReadInt32();
                    if (App.BinaryFileVersion == version)
                    {
                        dispatcher.Invoke(() => State = "Проверка сервера");
                        ulong serverId = binaryReader.ReadUInt64();
                        if (this.serverId.Equals(serverId))
                        {
                            int count = binaryReader.ReadInt32();
                            dispatcher.Invoke((Action<int>)((int scriptCount) => MaxValue = scriptCount), count);
                            for (int numScript = 0; numScript < count; numScript++)
                            {
                                dispatcher.Invoke(() => State = $"Чтение {Value + 1}/{MaxValue}");
                                Script script = new Script
                                {
                                    Channel = new Channel
                                    {
                                        Id = binaryReader.ReadUInt64(),
                                        Name = binaryReader.ReadString()
                                    },
                                    Mode = new Models.ScriptMode
                                    {
                                        Mode = (Enums.ScriptMode)binaryReader.ReadInt32()
                                    },
                                    MessageId = binaryReader.ReadUInt64(),
                                    Count = binaryReader.ReadInt32()
                                };
                                dispatcher.Invoke((Action<Script>)((Script invokeScript) =>
                                {
                                    State = $"Добавление {Value + 1}/{MaxValue}";
                                    scripts.Add(invokeScript);
                                    Value++;
                                }), script);
                            }
                        }
                        else
                        {
                            dispatcher.Invoke(() => UIC.MessageBox.Show("Выбран некорректный сервер!", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning));
                        }
                    }
                    else
                    {
                        dispatcher.Invoke(() => UIC.MessageBox.Show("Невозможно открыть файл т.к. его версия не поддерживается текущей версией программы.", App.Name, MessageBoxButton.OK, MessageBoxImage.Warning));
                    }
                }
            }
        }
    }
}