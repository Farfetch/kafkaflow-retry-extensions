using System.Collections.Generic;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.Postgres.Model;
using KafkaFlow.Retry.Postgres.Readers.Adapters;

namespace KafkaFlow.Retry.Postgres.Readers;

internal class RetryQueueReader : IRetryQueueReader
{
    private readonly IRetryQueueAdapter _retryQueueAdapter;
    private readonly IRetryQueueItemAdapter _retryQueueItemAdapter;
    private readonly IRetryQueueItemMessageAdapter _retryQueueItemMessageAdapter;
    private readonly IRetryQueueItemMessageHeaderAdapter _retryQueueItemMessageHeaderAdapter;

    public RetryQueueReader(
        IRetryQueueAdapter retryQueueAdapter,
        IRetryQueueItemAdapter retryQueueItemAdapter,
        IRetryQueueItemMessageAdapter retryQueueItemMessageAdapter,
        IRetryQueueItemMessageHeaderAdapter retryQueueItemMessageHeaderAdapter)
    {
        _retryQueueAdapter = retryQueueAdapter;
        _retryQueueItemAdapter = retryQueueItemAdapter;
        _retryQueueItemMessageAdapter = retryQueueItemMessageAdapter;
        _retryQueueItemMessageHeaderAdapter = retryQueueItemMessageHeaderAdapter;
    }

    public ICollection<RetryQueue> Read(RetryQueuesDboWrapper dboWrapper)
    {
        Guard.Argument(dboWrapper).NotNull();
        Guard.Argument(dboWrapper.QueuesDbos).NotNull();
        Guard.Argument(dboWrapper.ItemsDbos).NotNull();
        Guard.Argument(dboWrapper.MessagesDbos).NotNull();
        Guard.Argument(dboWrapper.HeadersDbos).NotNull();

        var retryQueues = new List<RetryQueue>();

        RetryQueueDbo previousRetryQueue = null;
        RetryQueue currentRetryQueue = null;

        var items = new DboCollectionNavigator<RetryQueueItemDbo, RetryQueueItem>(dboWrapper.ItemsDbos,
            _retryQueueItemAdapter);
        var messages =
            new DboCollectionNavigator<RetryQueueItemMessageDbo, RetryQueueItemMessage>(dboWrapper.MessagesDbos,
                _retryQueueItemMessageAdapter);
        var headers =
            new DboCollectionNavigator<RetryQueueItemMessageHeaderDbo, MessageHeader>(dboWrapper.HeadersDbos,
                _retryQueueItemMessageHeaderAdapter);

        foreach (var retryQueue in dboWrapper.QueuesDbos)
        {
            // check if we're still in the same retry queue as the previous
            if (previousRetryQueue is null || previousRetryQueue.Id != retryQueue.Id)
            {
                currentRetryQueue = _retryQueueAdapter.Adapt(retryQueue);

                retryQueues.Add(currentRetryQueue);
            }

            // navigate through all hierarchy and adapt and add to the retry queue
            items.Navigate(
                (item, itemDbo) => // reads all items
                {
                    messages.Navigate( // reads all messages (but only one because always the relation is 1 to 1)
                        (message, messageDbo) =>
                        {
                            item.Message = message;

                            headers.Navigate(
                                header => item.Message.AddHeader(header), // read each header
                                header => header.RetryQueueItemMessageId == messageDbo.IdRetryQueueItem);
                        },
                        messageDbo => messageDbo.IdRetryQueueItem == itemDbo.Id
                    );

                    currentRetryQueue.AddItem(item);
                },
                item => item.RetryQueueId == retryQueue.Id
            );

            // update previous retry queue as the current one
            previousRetryQueue = retryQueue;
        }

        return retryQueues;
    }
}