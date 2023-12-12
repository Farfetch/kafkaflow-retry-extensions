using System.Diagnostics.CodeAnalysis;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Update;

[ExcludeFromCodeCoverage]
public class UpdateItemsInQueueInput
{
    public UpdateItemsInQueueInput(string queueGroupKey, RetryQueueItemStatus status)
    {
            Guard.Argument(queueGroupKey, nameof(queueGroupKey)).NotNull();
            Guard.Argument(status, nameof(status)).NotDefault();

            this.QueueGroupKey = queueGroupKey;
            this.ItemStatus = status;
        }

    public RetryQueueItemStatus ItemStatus { get; }

    public string QueueGroupKey { get; }
}