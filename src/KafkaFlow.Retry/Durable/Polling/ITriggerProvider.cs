using KafkaFlow.Retry.Durable.Definitions.Polling;
using Quartz;

namespace KafkaFlow.Retry.Durable.Polling;

internal interface ITriggerProvider
{
    ITrigger GetPollingTrigger(string schedulerId, PollingDefinition pollingDefinition);
}