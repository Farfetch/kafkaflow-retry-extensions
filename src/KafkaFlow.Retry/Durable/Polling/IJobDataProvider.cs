using KafkaFlow.Retry.Durable.Definitions.Polling;
using Quartz;

namespace KafkaFlow.Retry.Durable.Polling;

internal interface IJobDataProvider
{
    IJobDetail JobDetail { get; }
    PollingDefinition PollingDefinition { get; }

    ITrigger Trigger { get; }
}