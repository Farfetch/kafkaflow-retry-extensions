namespace KafkaFlow.Retry.Durable.Polling
{
    using System.Collections.Generic;
    using KafkaFlow.Retry.Durable.Definitions.Polling;
    using Quartz;

    internal class TriggerProvider : ITriggerProvider
    {
        private readonly IDictionary<string, ITrigger> triggers;

        public TriggerProvider()
        {
            this.triggers = new Dictionary<string, ITrigger>();
        }

        public ITrigger GetPollingTrigger(string schedulerId, PollingDefinition pollingDefinition)
        {
            var triggerId = $"pollingJobTrigger_{schedulerId}_{pollingDefinition.PollingJobType}";

            if (this.triggers.TryGetValue(triggerId, out var triggerAlreadyCreated))
            {
                return triggerAlreadyCreated;
            }

            var trigger = TriggerBuilder
                            .Create()
                            .WithIdentity(triggerId, "queueTrackerGroup")
                            .WithCronSchedule(pollingDefinition.CronExpression)
                            .StartNow()
                            .WithPriority(1)
                            .Build();

            this.triggers.Add(triggerId, trigger);

            return trigger;
        }
    }
}