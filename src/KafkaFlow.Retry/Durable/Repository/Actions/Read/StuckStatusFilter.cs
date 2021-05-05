namespace KafkaFlow.Retry.Durable.Repository.Actions.Read
{
    using System;
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository.Model;

    public class StuckStatusFilter
    {
        public StuckStatusFilter(RetryQueueItemStatus itemStatus, TimeSpan expirationInterval)
        {
            Guard.Argument(itemStatus, nameof(itemStatus)).NotDefault();
            Guard.Argument(expirationInterval, nameof(expirationInterval)).NotZero().NotNegative();

            this.ItemStatus = itemStatus;
            this.ExpirationInterval = expirationInterval;
        }

        public TimeSpan ExpirationInterval { get; }
        public RetryQueueItemStatus ItemStatus { get; }
    }
}
