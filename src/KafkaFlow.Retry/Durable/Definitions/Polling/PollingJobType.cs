namespace KafkaFlow.Retry.Durable.Definitions.Polling;

internal enum PollingJobType
{
    Unknown = 0,

    RetryDurable = 1,
    Cleanup = 2,
    RetryDurableActiveQueuesCount = 3
}