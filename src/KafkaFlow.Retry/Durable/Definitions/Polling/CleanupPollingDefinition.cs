using Dawn;

namespace KafkaFlow.Retry.Durable.Definitions.Polling;

internal class CleanupPollingDefinition : PollingDefinition
{
    public CleanupPollingDefinition(
        bool enabled,
        string cronExpression,
        int timeToLiveInDays,
        int rowsPerRequest)
        : base(enabled, cronExpression)
    {
        if (enabled)
        {
            Guard.Argument(timeToLiveInDays, nameof(timeToLiveInDays)).Positive();
            Guard.Argument(rowsPerRequest, nameof(rowsPerRequest)).Positive();
        }

        TimeToLiveInDays = timeToLiveInDays;
        RowsPerRequest = rowsPerRequest;
    }

    public override PollingJobType PollingJobType => PollingJobType.Cleanup;

    public int RowsPerRequest { get; }

    public int TimeToLiveInDays { get; }
}