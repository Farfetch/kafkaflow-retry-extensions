namespace KafkaFlow.Retry.Durable.Repository.Actions.Delete
{
    using System.Diagnostics.CodeAnalysis;
    using Dawn;

    [ExcludeFromCodeCoverage]
    public class DeleteQueuesResult
    {
        public DeleteQueuesResult(int totalQueuesDeleted)
        {
            Guard.Argument(totalQueuesDeleted, nameof(totalQueuesDeleted)).NotNegative();

            this.TotalQueuesDeleted = totalQueuesDeleted;
        }

        public int TotalQueuesDeleted { get; }
    }
}