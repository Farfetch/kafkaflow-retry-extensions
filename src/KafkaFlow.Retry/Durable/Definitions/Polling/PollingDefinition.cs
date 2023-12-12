using Dawn;

namespace KafkaFlow.Retry.Durable.Definitions.Polling;

internal abstract class PollingDefinition
{
    protected PollingDefinition(bool enabled, string cronExpression)
    {
            Guard.Argument(this.PollingJobType, nameof(this.PollingJobType)).NotDefault();

            if (enabled)
            {
                Guard.Argument(Quartz.CronExpression.IsValidExpression(cronExpression), nameof(cronExpression))
                     .True("The cron expression that was defined is not valid");
            }

            this.Enabled = enabled;
            this.CronExpression = cronExpression;
        }

    public string CronExpression { get; }

    public bool Enabled { get; }

    public abstract PollingJobType PollingJobType { get; }
}