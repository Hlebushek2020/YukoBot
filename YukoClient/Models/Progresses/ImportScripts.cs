using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using YukoClientBase.Models.Progresses;
using MessageBox = YukoClientBase.Dialogs.MessageBox;

namespace YukoClient.Models.Progresses
{
    public class ImportScripts : BaseProgressModel
    {
        private readonly ICollection<Script> _scripts;
        private readonly string _fileName;
        private readonly ulong _serverId;

        public ImportScripts(ICollection<Script> scripts, ulong serverId, string fileName)
        {
            _scripts = scripts;
            _fileName = fileName;
            _serverId = serverId;
        }

        public override void Run(Dispatcher dispatcher, CancellationToken cancellationToken)
        {
            dispatcher.Invoke(() => State = "Подготовка к импорту правил");
            using (FileStream fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader binaryReader = new BinaryReader(fileStream, Encoding.UTF8))
                {
                    dispatcher.Invoke(() => State = "Проверка версии");
                    int version = binaryReader.ReadInt32();
                    if (App.BinaryFileVersion == version)
                    {
                        dispatcher.Invoke(() => State = "Проверка сервера");
                        ulong serverId = binaryReader.ReadUInt64();
                        if (_serverId.Equals(serverId))
                        {
                            int count = binaryReader.ReadInt32();
                            dispatcher.Invoke((Action<int>)((int scriptCount) => MaxValue = scriptCount), count);
                            for (int numScript = 0; numScript < count; numScript++)
                            {
                                dispatcher.Invoke(() => State = $"Чтение {Value + 1}/{MaxValue}");
                                Script script = new Script
                                {
                                    Channel = new Channel(binaryReader.ReadUInt64(), binaryReader.ReadString()),
                                    Mode = new ScriptMode((Enums.ScriptMode)binaryReader.ReadInt32()),
                                    MessageId = binaryReader.ReadUInt64(),
                                    Count = binaryReader.ReadInt32()
                                };
                                dispatcher.Invoke((Action<Script>)((Script invokeScript) =>
                                {
                                    State = $"Добавление {Value + 1}/{MaxValue}";
                                    _scripts.Add(invokeScript);
                                    Value++;
                                }), script);
                            }
                        }
                        else
                        {
                            dispatcher.Invoke(() => MessageBox.Show("Выбран некорректный сервер!", App.Name,
                                MessageBoxButton.OK, MessageBoxImage.Warning));
                        }
                    }
                    else
                    {
                        dispatcher.Invoke(() => MessageBox.Show(
                            "Невозможно открыть файл т.к. его версия не поддерживается текущей версией программы.",
                            App.Name, MessageBoxButton.OK, MessageBoxImage.Warning));
                    }
                }
            }
        }
    }
}