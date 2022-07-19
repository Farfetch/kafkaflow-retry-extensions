namespace KafkaFlow.Retry.Durable.Polling
{
    using Quartz;

    internal interface IJobDetailProvider
    {
        IJobDetail GetQueuePollingJobDetail();
    }
}