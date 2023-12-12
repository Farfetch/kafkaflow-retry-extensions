namespace KafkaFlow.Retry;

public abstract class PollingDefinitionBuilder<TSelf> where TSelf : PollingDefinitionBuilder<TSelf>
{
    protected string cronExpression;
    protected bool enabled;

    internal abstract bool Required { get; }

    public TSelf Enabled(bool value)
    {
            enabled = value;
            return (TSelf)this;
        }

    public TSelf WithCronExpression(string cronExpression)
    {
            this.cronExpression = cronExpression;
            return (TSelf)this;
        }
}