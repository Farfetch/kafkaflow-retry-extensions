using System;
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
        attemptsCount = 0;
        creationDate = RetryQueueBuilder.DefaultDateTime;
        this.sort = sort;
        lastExecution = RetryQueueBuilder.DefaultDateTime;
        modifiedStatusDate = RetryQueueBuilder.DefaultDateTime;
        status = RetryQueueItemStatus.Waiting;
        severityLevel = SeverityLevel.Medium;
        description = string.Empty;
        message = DefaultItemMessage;
    }

    public RetryQueueBuilder AddItem()
    {
        return retryQueueBuilder.WithItem(Build());
    }

    public RetryQueueItemBuilder WithDoneStatus()
    {
        return WithStatus(RetryQueueItemStatus.Done);
    }

    public RetryQueueItemBuilder WithInRetryStatus()
    {
        return WithStatus(RetryQueueItemStatus.InRetry);
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
        return WithStatus(RetryQueueItemStatus.Waiting);
    }

    private RetryQueueItem Build()
    {
        id = id == default ? Guid.NewGuid() : id;

        return new RetryQueueItem(
            id,
            attemptsCount,
            creationDate,
            sort,
            lastExecution,
            modifiedStatusDate,
            status,
            severityLevel,
            description)
        {
            Message = message
        };
    }
}