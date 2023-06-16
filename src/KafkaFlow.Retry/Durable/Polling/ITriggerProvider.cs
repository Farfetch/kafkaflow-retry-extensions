namespace KafkaFlow.Retry.Durable.Polling
{
    using Quartz;

    internal interface ITriggerProvider
    {
        ITrigger GetQueuePollingTrigger();
    }
}