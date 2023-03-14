namespace KafkaFlow.Retry.Durable.Polling
{
    using KafkaFlow.Retry.Durable.Definitions.Polling;
    using Quartz;

    internal interface IJobDataProvider
    {
        IJobDetail JobDetail { get; }
        PollingDefinition PollingDefinition { get; }

        ITrigger Trigger { get; }
    }
}