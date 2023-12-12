using KafkaFlow.Retry.Durable.Definitions.Polling;
using Quartz;

namespace KafkaFlow.Retry.Durable.Polling;

internal class TriggerProvider : ITriggerProvider
{
    public ITrigger GetPollingTrigger(string schedulerId, PollingDefinition pollingDefinition)
        => TriggerBuilder
            .Create()
            .WithIdentity($"pollingJobTrigger_{schedulerId}_{pollingDefinition.PollingJobType}", "queueTrackerGroup")
            .WithCronSchedule(pollingDefinition.CronExpression, cronBuilder => cronBuilder.WithMisfireHandlingInstructionDoNothing())
            .StartNow()
            .WithPriority(1)
            .Build();
}