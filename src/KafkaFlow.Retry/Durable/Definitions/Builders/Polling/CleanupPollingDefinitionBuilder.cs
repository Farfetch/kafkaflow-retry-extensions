using KafkaFlow.Retry.Durable.Definitions.Polling;

namespace KafkaFlow.Retry;

public class CleanupPollingDefinitionBuilder : PollingDefinitionBuilder<CleanupPollingDefinitionBuilder>
{
    private int _rowsPerRequest = 256;
    private int _timeToLiveInDays = 30;

    internal override bool Required => false;

    public CleanupPollingDefinitionBuilder WithRowsPerRequest(int rowsPerRequest)
    {
            _rowsPerRequest = rowsPerRequest;
            return this;
        }

    public CleanupPollingDefinitionBuilder WithTimeToLiveInDays(int timeToLiveInDays)
    {
            _timeToLiveInDays = timeToLiveInDays;
            return this;
        }

    internal CleanupPollingDefinition Build()
    {
            return new CleanupPollingDefinition(
                IsEnabled,
                CronExpression,
                _timeToLiveInDays,
                _rowsPerRequest
            );
        }
}