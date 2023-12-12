using System;
using System.Diagnostics.CodeAnalysis;
using Dawn;

namespace KafkaFlow.Retry.Durable.Repository.Actions.Read;

[ExcludeFromCodeCoverage]
public class QueuePendingItemsInput
{
    public QueuePendingItemsInput(Guid queueId, Guid itemId, int sort)
    {
            Guard.Argument(queueId, nameof(queueId)).NotDefault();
            Guard.Argument(itemId, nameof(itemId)).NotDefault();
            Guard.Argument(sort, nameof(sort)).NotNegative();

            this.QueueId = queueId;
            this.ItemId = itemId;
            this.Sort = sort;
        }

    public Guid ItemId { get; }
    public Guid QueueId { get; }
    public int Sort { get; }
}