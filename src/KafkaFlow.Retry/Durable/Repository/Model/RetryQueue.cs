using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Dawn;

namespace KafkaFlow.Retry.Durable.Repository.Model;

[ExcludeFromCodeCoverage]
public class RetryQueue
{
    private readonly SortedList<int, RetryQueueItem> itemsList;

    public RetryQueue(
        Guid id,
        string searchGroupKey,
        string queueGroupKey,
        DateTime creationDate,
        DateTime lastExecution,
        RetryQueueStatus status,
        IEnumerable<RetryQueueItem> items = null)
    {
            Guard.Argument(id, nameof(id)).NotDefault();
            Guard.Argument(searchGroupKey, nameof(searchGroupKey)).NotNull().NotEmpty();
            Guard.Argument(queueGroupKey, nameof(queueGroupKey)).NotNull().NotEmpty();
            Guard.Argument(creationDate, nameof(creationDate)).NotDefault();
            Guard.Argument(lastExecution, nameof(lastExecution)).NotDefault();
            Guard.Argument(status, nameof(status)).NotDefault();

            this.Id = id;
            this.SearchGroupKey = searchGroupKey;
            this.QueueGroupKey = queueGroupKey;
            this.CreationDate = creationDate;
            this.LastExecution = lastExecution;
            this.Status = status;

            this.itemsList = items is null ? new SortedList<int, RetryQueueItem>() : new SortedList<int, RetryQueueItem>(items.ToDictionary(i => i.Sort));
        }

    public DateTime CreationDate { get; }

    public Guid Id { get; }

    public IEnumerable<RetryQueueItem> Items { get => this.itemsList.Values; }

    public DateTime LastExecution { get; }

    public string QueueGroupKey { get; }

    public string SearchGroupKey { get; }

    public RetryQueueStatus Status { get; }

    public void AddItem(RetryQueueItem item)
    {
            this.itemsList.Add(item.Sort, item);
        }
}