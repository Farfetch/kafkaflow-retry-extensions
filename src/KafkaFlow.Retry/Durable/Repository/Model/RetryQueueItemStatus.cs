namespace KafkaFlow.Retry.Durable.Repository.Model;

public enum RetryQueueItemStatus
{
    None = 0,
    Waiting = 1,
    InRetry = 2,
    Done = 3,
    Cancelled = 4
}