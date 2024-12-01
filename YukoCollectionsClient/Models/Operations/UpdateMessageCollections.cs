using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using YukoClientBase.Args;
using YukoClientBase.Exceptions;
using YukoClientBase.Models.Operations;
using YukoCollectionsClient.Models.Web;
using YukoCollectionsClient.Models.Web.Responses;

namespace YukoCollectionsClient.Models.Operations
{
    public class UpdateMessageCollections : IOperation
    {
        private readonly bool _overrideMessageCollections;

        public UpdateMessageCollections(bool overrideMessageCollections)
        {
            _overrideMessageCollections = overrideMessageCollections;
        }

        public Task Run(IProgress<ProgressReportArgs> progress, CancellationToken cancellationToken)
        {
            progress.Report(new ProgressReportArgs { IsIndeterminate = true, Text = "Получение данных" });
            MessageCollectionsResponse response = YukoWebClient.Current.GetMessageCollections();

            if (response.Error != null)
                throw new ClientCodeException(response.Error.Code);

            cancellationToken.ThrowIfCancellationRequested();

            if (_overrideMessageCollections)
            {
                progress.Report(new ProgressReportArgs { Text = "Перезапись коллекций" });

                Storage.Current.MessageCollections =
                    new ObservableCollection<MessageCollection>(response.MessageCollections);
            }
            else
            {
                foreach (MessageCollection collectionResp in response.MessageCollections)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    progress.Report(new ProgressReportArgs { Text = $"Обработка коллекции \"{collectionResp.Name}\"" });

                    if (Storage.Current.MessageCollections.Contains(collectionResp))
                    {
                        int index = Storage.Current.MessageCollections.IndexOf(collectionResp);
                        MessageCollection messageCollection = Storage.Current.MessageCollections[index];
                        foreach (MessageCollectionItem itemResp in collectionResp.Items)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            if (!messageCollection.Items.Contains(itemResp))
                                messageCollection.Items.Add(itemResp);
                        }
                    }
                    else
                    {
                        Storage.Current.MessageCollections.Add(collectionResp);
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}