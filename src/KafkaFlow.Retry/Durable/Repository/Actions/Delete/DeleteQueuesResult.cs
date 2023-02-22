namespace KafkaFlow.Retry.Durable.Repository.Actions.Delete
{
    using Dawn;

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