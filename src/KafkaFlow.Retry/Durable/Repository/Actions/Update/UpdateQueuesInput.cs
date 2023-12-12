using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Durable.Repository;

[ExcludeFromCodeCoverage]
public class UpdateQueuesInput
{
    public UpdateQueuesInput(IEnumerable<string> queueGroupKeys, RetryQueueItemStatus status)
    {
        Guard.Argument(queueGroupKeys).NotNull().NotEmpty();
        Guard.Argument(status).NotDefault();

        QueueGroupKeys = queueGroupKeys;
        ItemStatus = status;
    }

    public RetryQueueItemStatus ItemStatus { get; }

    public IEnumerable<string> QueueGroupKeys { get; }
}