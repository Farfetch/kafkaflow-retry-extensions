using Dawn;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Delete;

public class DeleteQueuesResult
{
    public DeleteQueuesResult(int totalQueuesDeleted)
    {
            Guard.Argument(totalQueuesDeleted, nameof(totalQueuesDeleted)).NotNegative();

            TotalQueuesDeleted = totalQueuesDeleted;
        }

    public int TotalQueuesDeleted { get; }
}