namespace KafkaFlow.Retry;

public abstract class PollingDefinitionBuilder<TSelf> where TSelf : PollingDefinitionBuilder<TSelf>
{
    protected string CronExpression;
    protected bool IsEnabled;

    internal abstract bool Required { get; }

    public TSelf Enabled(bool value)
    {
        IsEnabled = value;
        return (TSelf)this;
    }

    public TSelf WithCronExpression(string cronExpression)
    {
        CronExpression = cronExpression;
        return (TSelf)this;
    }
}