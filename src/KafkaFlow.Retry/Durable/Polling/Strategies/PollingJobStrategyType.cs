namespace KafkaFlow.Retry.Durable.Polling.Strategies
{
    internal enum PollingJobStrategyType
    {
        None = 0,
        Earliest = 1,
        Latest = 2
    }
}