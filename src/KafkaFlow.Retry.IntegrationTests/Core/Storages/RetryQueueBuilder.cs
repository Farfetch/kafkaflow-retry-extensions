﻿using System;
using System.Collections.Generic;
using System.Linq;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Actions.Create;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.IntegrationTests.Core.Storages;

internal class RetryQueueBuilder
{
    public static readonly DateTime DefaultDateTime = new DateTime(2020, 5, 25).ToUniversalTime();

    private readonly List<RetryQueueItem> items;
    private DateTime creationDate;
    private DateTime lastExecution;
    private string queueGroupKey;
    private Guid queueId;
    private string searchGroupKey;
    private RetryQueueStatus status;

    public RetryQueueBuilder()
    {
        // defaults
        queueId = Guid.NewGuid();
        searchGroupKey = "default-search-group-key-repositories-tests";
        queueGroupKey = $"queue-group-key-{queueId}";
        status = RetryQueueStatus.Active;
        creationDate = DefaultDateTime;
        lastExecution = DefaultDateTime;

        items = new List<RetryQueueItem>();
    }

    public RetryQueue Build()
    {
        return new RetryQueue(
            queueId,
            searchGroupKey,
            queueGroupKey,
            DefaultDateResilience(creationDate),
            DefaultDateResilience(lastExecution),
            status,
            items
        );
    }

    public SaveToQueueInput BuildAsInput()
    {
        Guard.Argument(items, nameof(items)).Count(1);

        var item = items.Single();

        return new SaveToQueueInput(
            item.Message,
            searchGroupKey,
            queueGroupKey,
            status,
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
        return new RetryQueueItemBuilder(this, items.Count);
    }

    public RetryQueueBuilder WithCreationDate(DateTime creationDate)
    {
        this.creationDate = creationDate;
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
        items.Add(item);
        return this;
    }

    public RetryQueueBuilder WithItems(RetryQueueItem[] items)
    {
        this.items.AddRange(items);
        return this;
    }

    public RetryQueueBuilder WithLastExecution(DateTime lastExecution)
    {
        this.lastExecution = lastExecution;
        return this;
    }

    public RetryQueueBuilder WithQueueGroupKey(string queueGroupKey)
    {
        this.queueGroupKey = queueGroupKey;
        return this;
    }

    public RetryQueueBuilder WithQueueId(Guid queueId)
    {
        this.queueId = queueId;
        return this;
    }

    public RetryQueueBuilder WithSearchGroupKey(string searchGroupKey)
    {
        this.searchGroupKey = searchGroupKey;
        return this;
    }

    public RetryQueueBuilder WithStatus(RetryQueueStatus status)
    {
        this.status = status;
        return this;
    }

    private DateTime DefaultDateResilience(DateTime date)
    {
        return date == default ? DefaultDateTime : date;
    }
}