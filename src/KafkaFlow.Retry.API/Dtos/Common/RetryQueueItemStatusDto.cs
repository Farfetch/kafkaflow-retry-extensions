namespace KafkaFlow.Retry.API.Dtos.Common
{
    public enum RetryQueueItemStatusDto
    {
        None = 0,
        Waiting = 1,
        InRetry = 2,
        Done = 3,
        Cancelled = 4
    }
}
