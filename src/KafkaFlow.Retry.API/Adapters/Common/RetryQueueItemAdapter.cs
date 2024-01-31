using Dawn;
using KafkaFlow.Retry.API.Dtos.Common;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.API.Adapters.Common;

internal class RetryQueueItemAdapter : IRetryQueueItemAdapter
{
    public RetryQueueItemDto Adapt(RetryQueueItem item, string queueGroupKey)
    {
        Guard.Argument(item, nameof(item)).NotNull();
        Guard.Argument(item.Message, nameof(item.Message)).NotNull();

        return new RetryQueueItemDto
        {
            Id = item.Id,
            Status = item.Status,
            SeverityLevel = item.SeverityLevel,
            AttemptsCount = item.AttemptsCount,
            CreationDate = item.CreationDate,
            LastExecution = item.LastExecution,
            Sort = item.Sort,
            MessageInfo = new RetryQueuetItemMessageInfoDto
            {
                Key = item.Message.Key,
                Offset = item.Message.Offset,
                Partition = item.Message.Partition,
                Topic = item.Message.TopicName,
                UtcTimeStamp = item.Message.UtcTimeStamp
            },
            Description = item.Description,
            QueueGroupKey = queueGroupKey
        };
    }
}