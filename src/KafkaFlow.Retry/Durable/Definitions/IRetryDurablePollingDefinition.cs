namespace KafkaFlow.Retry.Durable.Definitions
{
    internal interface IRetryDurablePollingDefinition
    {
        string CronExpression { get; }

        bool Enabled { get; }

        int ExpirationIntervalFactor { get; }

        int FetchSize { get; }

        string Id { get; }
    }
}