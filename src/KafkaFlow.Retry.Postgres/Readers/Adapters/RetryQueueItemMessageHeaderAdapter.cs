using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.Postgres.Model;

namespace KafkaFlow.Retry.Postgres.Readers.Adapters;

internal class RetryQueueItemMessageHeaderAdapter : IRetryQueueItemMessageHeaderAdapter
{
    public MessageHeader Adapt(RetryQueueItemMessageHeaderDbo messageHeaderDbo)
    {
        Guard.Argument(messageHeaderDbo).NotNull();

        return new MessageHeader(messageHeaderDbo.Key, messageHeaderDbo.Value);
    }
}