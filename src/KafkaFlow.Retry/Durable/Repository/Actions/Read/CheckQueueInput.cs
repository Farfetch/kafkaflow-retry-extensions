using System.Diagnostics.CodeAnalysis;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Read;

[ExcludeFromCodeCoverage]
public class CheckQueueInput
{
    public CheckQueueInput(RetryQueueItemMessage message, string queueGroupKey)
    {
            Guard.Argument(message).NotNull();
            Guard.Argument(queueGroupKey).NotNull().NotEmpty();

            this.Message = message;
            this.QueueGroupKey = queueGroupKey;
        }

    public RetryQueueItemMessage Message { get; }

    public string QueueGroupKey { get; }
}