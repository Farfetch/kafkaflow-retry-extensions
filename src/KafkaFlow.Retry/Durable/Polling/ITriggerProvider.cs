namespace KafkaFlow.Retry.Durable.Polling
{
    using KafkaFlow.Retry.Durable.Definitions.Polling;
    using Quartz;

    internal interface ITriggerProvider
    {
        ITrigger GetPollingTrigger(string schedulerId, PollingDefinition pollingDefinition);
    }
}