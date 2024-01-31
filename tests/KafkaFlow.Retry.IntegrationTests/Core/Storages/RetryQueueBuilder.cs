using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Actions.Create;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.IntegrationTests.Core.Storages;

internal class RetryQueueBuilder
{
    public static readonly DateTime DefaultDateTime = new DateTime(2020, 5, 25).ToUniversalTime();

    private readonly List<RetryQueueItem> _items;
    private DateTime _creationDate;
    private DateTime _lastExecution;
    private string _queueGroupKey;
    private Guid _queueId;
    private string _searchGroupKey;
    private RetryQueueStatus _status;

    public RetryQueueBuilder()
    {
        // defaults
        _queueId = Guid.NewGuid();
        _searchGroupKey = "default-search-group-key-repositories-tests";
        _queueGroupKey = $"queue-group-key-{_queueId}";
        _status = RetryQueueStatus.Active;
        _creationDate = DefaultDateTime;
        _lastExecution = DefaultDateTime;

        _items = new List<RetryQueueItem>();
    }

    public RetryQueue Build()
    {
        return new RetryQueue(
            _queueId,
            _searchGroupKey,
            _queueGroupKey,
            DefaultDateResilience(_creationDate),
            DefaultDateResilience(_lastExecution),
            _status,
            _items
        );
    }

    public SaveToQueueInput BuildAsInput()
    {
        Guard.Argument(_items, nameof(_items)).Count(1);

        var item = _items.Single();

        return new SaveToQueueInput(
            item.Message,
            _searchGroupKey,
            _queueGroupKey,
            _status,
            item.Status,
            item.SeverityLevel,
            item.CreationDate,
            item.LastExecution,
            item.ModifiedStatusDate,
            item.AttemptsCount,
            item.Description
        );
    }

    public RetryQueueItemBuilder CreateItem()
    {
        return new RetryQueueItemBuilder(this, _items.Count);
    }

    public RetryQueueBuilder WithCreationDate(DateTime creationDate)
    {
        _creationDate = creationDate;
        return this;
    }

    public RetryQueueBuilder WithDefaultItem()
    {
        return CreateItem()
            .WithWaitingStatus()
            .AddItem();
    }

    public RetryQueueBuilder WithItem(RetryQueueItem item)
    {
        _items.Add(item);
        return this;
    }

    public RetryQueueBuilder WithItems(RetryQueueItem[] items)
    {
        _items.AddRange(items);
        return this;
    }

    public RetryQueueBuilder WithLastExecution(DateTime lastExecution)
    {
        _lastExecution = lastExecution;
        return this;
    }

    public RetryQueueBuilder WithQueueGroupKey(string queueGroupKey)
    {
        _queueGroupKey = queueGroupKey;
        return this;
    }

    public RetryQueueBuilder WithQueueId(Guid queueId)
    {
        _queueId = queueId;
        return this;
    }

    public RetryQueueBuilder WithSearchGroupKey(string searchGroupKey)
    {
        _searchGroupKey = searchGroupKey;
        return this;
    }

    public RetryQueueBuilder WithStatus(RetryQueueStatus status)
    {
        _status = status;
        return this;
    }

    private DateTime DefaultDateResilience(DateTime date)
    {
        return date == default ? DefaultDateTime : date;
    }
}