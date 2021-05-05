namespace KafkaFlow.Retry.Durable.Repository.Actions.Read
{
    using System.Diagnostics.CodeAnalysis;
    using Dawn;

    [ExcludeFromCodeCoverage]
    public class QueuePendingItemsResult
    {
        public QueuePendingItemsResult(QueuePendingItemsResultStatus status)
        {
            Guard.Argument(status, nameof(status)).NotDefault();

            this.Status = status;
        }

        public QueuePendingItemsResultStatus Status { get; }
    }
}
