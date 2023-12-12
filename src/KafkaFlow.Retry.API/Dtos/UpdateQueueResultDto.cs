using KafkaFlow.Retry.Durable.Repository.Actions.Update;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.API.Dtos;

public class UpdateQueueResultDto
{
    public UpdateQueueResultDto(string queueGroupKey, UpdateQueueResultStatus status, RetryQueueStatus retryQueueStatus)
    {
        QueueGroupKey = queueGroupKey;
        Result = status.ToString();
        QueueStatus = retryQueueStatus.ToString();
    }

    public string QueueGroupKey { get; set; }

    public string QueueStatus { get; set; }

    public string Result { get; set; }
}