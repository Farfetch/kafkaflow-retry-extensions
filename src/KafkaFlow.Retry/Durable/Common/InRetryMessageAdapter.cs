namespace KafkaFlow.Retry.Durable.Common
{
    using System;
    using System.Linq;
    using Dawn;
    using KafkaFlow.Retry.Durable.Contracts;
    using KafkaFlow.Retry.Durable.Repository.Model;

    internal class InRetryMessageAdapter
    {
        public InRetryMessage Adapt(Guid queueId, RetryQueueItem item)
        {
            Guard.Argument(queueId).NotDefault();
            Guard.Argument(item).NotNull();

            return
                new InRetryMessage()
                {
                    QueueId = queueId,
                    ItemId = item.Id,
                    AttemptsCount = item.AttemptsCount,
                    Sort = item.Sort,
                    OriginalMessage = new OriginalMessage()
                    {
                        Key = item.Message.Key,
                        Value = item.Message.Value,
                        TopicName = item.Message.TopicName,
                        Partition = item.Message.Partition,
                        Offset = item.Message.Offset,
                        UtcTimeStamp = item.Message.UtcTimeStamp,
                        Headers = item.Message.Headers?.Select
                        (
                            h => new OriginalMessageHeader()
                            {
                                Key = h.Key,
                                Value = h.Value
                            }
                        )
                    }
                };
        }
    }
}