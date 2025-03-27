using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Read;
public class CountQueuesInput
{
    public CountQueuesInput(RetryQueueStatus status)
    {
        Guard.Argument(status, nameof(status)).NotDefault();

        Status = status;
    }
    public RetryQueueStatus Status { get; }
    public string SearchGroupKey { get; set; }
}
