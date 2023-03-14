namespace KafkaFlow.Retry.Durable.Polling
{
    using KafkaFlow.Retry.Durable.Definitions.Polling;
    using Quartz;

    internal class TriggerProvider : ITriggerProvider
    {
        public ITrigger GetPollingTrigger(string schedulerId, PollingDefinition pollingDefinition)
            => TriggerBuilder
                .Create()
                .WithIdentity($"pollingJobTrigger_{schedulerId}_{pollingDefinition.PollingJobType}", "queueTrackerGroup")
                .WithCronSchedule(pollingDefinition.CronExpression)
                .StartNow()
                .WithPriority(1)
                .Build();
    }
}