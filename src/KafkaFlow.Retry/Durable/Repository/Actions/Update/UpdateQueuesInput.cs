namespace KafkaFlow.Retry.Durable.Repository
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository.Model;

    [ExcludeFromCodeCoverage]
    public class UpdateQueuesInput
    {
        public UpdateQueuesInput(IEnumerable<string> queueGroupKeys, RetryQueueItemStatus status)
        {
            Guard.Argument(queueGroupKeys).NotNull().NotEmpty();
            Guard.Argument(status).NotDefault();

            this.QueueGroupKeys = queueGroupKeys;
            this.ItemStatus = status;
        }

        public RetryQueueItemStatus ItemStatus { get; }

        public IEnumerable<string> QueueGroupKeys { get; }
    }
}
