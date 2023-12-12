using System;
using System.Diagnostics.CodeAnalysis;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Update;

[ExcludeFromCodeCoverage]
public abstract class UpdateItemInput
{
    protected UpdateItemInput(Guid itemId, RetryQueueItemStatus status)
    {
            Guard.Argument(itemId, nameof(itemId)).NotDefault();
            Guard.Argument(status, nameof(status)).NotDefault();

            this.ItemId = itemId;
            this.Status = status;
        }

    public Guid ItemId { get; }
    public RetryQueueItemStatus Status { get; }
}