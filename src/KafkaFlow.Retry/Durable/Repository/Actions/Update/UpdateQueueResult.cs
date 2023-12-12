using System.Diagnostics.CodeAnalysis;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Update;

[ExcludeFromCodeCoverage]
public class UpdateQueueResult
{
    public UpdateQueueResult(string queueGroupKey, UpdateQueueResultStatus updateStatus, RetryQueueStatus retryQueueStatus)
    {
            Guard.Argument(queueGroupKey, nameof(queueGroupKey)).NotNull();
            Guard.Argument(updateStatus, nameof(updateStatus)).NotDefault();

            this.QueueGroupKey = queueGroupKey;
            this.Status = updateStatus;
            this.RetryQueueStatus = retryQueueStatus;
        }

    public string QueueGroupKey { get; }
    public RetryQueueStatus RetryQueueStatus { get; }
    public UpdateQueueResultStatus Status { get; }
}