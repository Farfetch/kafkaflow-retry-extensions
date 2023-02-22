namespace KafkaFlow.Retry
{
    public abstract class PollingDefinitionBuilder<T> where T : PollingDefinitionBuilder<T>
    {
        protected string cronExpression;
        protected bool enabled;

        internal abstract bool Required { get; }

        public T Enabled(bool value)
        {
            this.enabled = value;
            return (T)this;
        }

        public T WithCronExpression(string cronExpression)
        {
            this.cronExpression = cronExpression;
            return (T)this;
        }
    }
}