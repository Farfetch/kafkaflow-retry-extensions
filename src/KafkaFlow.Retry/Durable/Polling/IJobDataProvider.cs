namespace KafkaFlow.Retry.Durable.Polling
{
    using KafkaFlow.Retry.Durable.Definitions.Polling;
    using Quartz;

    internal interface IJobDataProvider
    {
        PollingDefinition PollingDefinition { get; }

        IJobDetail GetPollingJobDetail();
    }
}