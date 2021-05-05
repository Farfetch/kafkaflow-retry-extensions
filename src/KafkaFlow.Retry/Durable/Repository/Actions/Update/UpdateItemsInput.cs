namespace KafkaFlow.Retry.Durable.Repository.Actions.Update
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository.Model;

    [ExcludeFromCodeCoverage]
    public class UpdateItemsInput
    {
        public UpdateItemsInput(IEnumerable<Guid> itemIds, RetryQueueItemStatus status)
        {
            Guard.Argument(itemIds, nameof(itemIds)).NotNull().NotEmpty();
            Guard.Argument(status, nameof(status)).NotDefault();

            this.ItemIds = itemIds;
            this.Status = status;
        }

        public IEnumerable<Guid> ItemIds { get; }
        public RetryQueueItemStatus Status { get; }
    }
}
