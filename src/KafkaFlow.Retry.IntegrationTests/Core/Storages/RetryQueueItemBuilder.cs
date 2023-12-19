﻿using System;
using System.Collections.Generic;
using Dawn;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.IntegrationTests.Core.Storages;

internal class RetryQueueItemBuilder
{
    public static readonly RetryQueueItemMessage DefaultItemMessage = new RetryQueueItemMessage(
        "DefaultTopicNameForTests",
        new byte[1],
        new byte[2],
        0,
        0,
        RetryQueueBuilder.DefaultDateTime,
        new List<MessageHeader> { new MessageHeader("headerKey1", new byte[3]) }
    );

    private readonly int attemptsCount;
    private readonly DateTime creationDate;
    private readonly string description;
    private readonly RetryQueueItemMessage message;
    private readonly RetryQueueBuilder retryQueueBuilder;
    private readonly int sort;
    private Guid id;
    private DateTime? lastExecution;
    private DateTime? modifiedStatusDate;
    private SeverityLevel severityLevel;
    private RetryQueueItemStatus status;

    public RetryQueueItemBuilder(RetryQueueBuilder retryQueueBuilder, int sort)
    {
        Guard.Argument(retryQueueBuilder, nameof(retryQueueBuilder)).NotNull();

        this.retryQueueBuilder = retryQueueBuilder;

        // defaults
        this.attemptsCount = 0;
        this.creationDate = RetryQueueBuilder.DefaultDateTime;
        this.sort = sort;
        this.lastExecution = RetryQueueBuilder.DefaultDateTime;
        this.modifiedStatusDate = RetryQueueBuilder.DefaultDateTime;
        this.status = RetryQueueItemStatus.Waiting;
        this.severityLevel = SeverityLevel.Medium;
        this.description = string.Empty;
        this.message = DefaultItemMessage;
    }

    public RetryQueueBuilder AddItem()
    {
        return this.retryQueueBuilder.WithItem(this.Build());
    }

    public RetryQueueItemBuilder WithDoneStatus()
    {
        return this.WithStatus(RetryQueueItemStatus.Done);
    }

    public RetryQueueItemBuilder WithInRetryStatus()
    {
        return this.WithStatus(RetryQueueItemStatus.InRetry);
    }

    public RetryQueueItemBuilder WithModifiedStatusDate(DateTime? modifiedStatusDate)
    {
        this.modifiedStatusDate = modifiedStatusDate;

        return this;
    }

    public RetryQueueItemBuilder WithSeverityLevel(SeverityLevel severityLevel)
    {
        this.severityLevel = severityLevel;

        return this;
    }

    public RetryQueueItemBuilder WithStatus(RetryQueueItemStatus status)
    {
        this.status = status;

        return this;
    }

    public RetryQueueItemBuilder WithWaitingStatus()
    {
        return this.WithStatus(RetryQueueItemStatus.Waiting);
    }

    private RetryQueueItem Build()
    {
        this.id = this.id == default ? Guid.NewGuid() : this.id;

        return new RetryQueueItem(
            this.id,
            this.attemptsCount,
            this.creationDate,
            this.sort,
            this.lastExecution,
            this.modifiedStatusDate,
            this.status,
            this.severityLevel,
            this.description)
        {
            Message = this.message
        };
    }
}