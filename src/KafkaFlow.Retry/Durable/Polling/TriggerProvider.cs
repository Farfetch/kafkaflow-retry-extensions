namespace KafkaFlow.Retry.Durable.Polling
{
    using Dawn;
    using KafkaFlow.Retry.Durable.Definitions;
    using Quartz;

    internal class TriggerProvider : ITriggerProvider
    {
        private readonly RetryDurablePollingDefinition retryDurablePollingDefinition;

        public TriggerProvider(RetryDurablePollingDefinition retryDurablePollingDefinition)
        {
            Guard.Argument(retryDurablePollingDefinition).NotNull();

            this.retryDurablePollingDefinition = retryDurablePollingDefinition;
        }

        public ITrigger GetQueuePollingTrigger()
        {
            return TriggerBuilder
                .Create()
                .WithIdentity($"pollingJob_{this.retryDurablePollingDefinition.Id}", "queueTrackerGroup")
                .WithCronSchedule(this.retryDurablePollingDefinition.CronExpression)
                .StartNow()
                .WithPriority(1)
                .Build();
        }
    }
}