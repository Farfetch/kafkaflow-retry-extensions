namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Models
{
    public enum RetryQueueItemStatusTestModel
    {
        None = 0,
        Waiting = 1,
        InRetry = 2,
        Done = 3,
        Cancelled = 4
    }
}