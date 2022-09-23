//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.IO;
//using System.Text;

//namespace YukoBot
//{
//    internal static class MessageStore
//    {
//        private static string _storeFolder = YukoSettings.Current.GetMessageStoryDirectory();

//        private class LookObject
//        {
//            public object look = new object();
//        }

//        private static ConcurrentDictionary<string, LookObject> _lookObjects = new ConcurrentDictionary<string, LookObject>();

//        public static void Save(IEnumerable<string> links, ulong channelId, ulong messageId)
//        {
//            string channelDir = Path.Combine(_storeFolder, channelId.ToString());
//            Directory.CreateDirectory(channelDir);
//            string key = channelId + "|" + messageId;
//            lock (_lookObjects.GetOrAdd(key, new LookObject()).look)
//            {
//                using (StreamWriter streamWriter = new StreamWriter(Path.Combine(channelDir, $"{messageId}.cache"), false, Encoding.UTF8))
//                {
//                    foreach (string link in links)
//                    {
//                        streamWriter.WriteLine(link);
//                    }
//                }
//            }
//        }

//        public static IEnumerable<string> Get(ulong channelId, ulong messageId)
//        {
//            return new List<string>();
//        }
//    }
//}