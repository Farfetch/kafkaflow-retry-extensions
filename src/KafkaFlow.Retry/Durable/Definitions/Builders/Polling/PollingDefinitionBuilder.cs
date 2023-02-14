namespace KafkaFlow.Retry
{
    public abstract class PollingDefinitionBuilder<SELF> where SELF : PollingDefinitionBuilder<SELF>
    {
        protected string cronExpression;
        protected bool enabled;

        public SELF Enabled(bool enabled)
        {
            this.enabled = enabled;
            return (SELF)this;
        }

        public SELF WithCronExpression(string cronExpression)
        {
            this.cronExpression = cronExpression;
            return (SELF)this;
        }
    }
}