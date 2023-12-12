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

    private readonly int _attemptsCount;
    private readonly DateTime _creationDate;
    private readonly string _description;
    private readonly RetryQueueItemMessage _message;
    private readonly RetryQueueBuilder _retryQueueBuilder;
    private readonly int _sort;
    private Guid _id;
    private DateTime? _lastExecution;
    private DateTime? _modifiedStatusDate;
    private SeverityLevel _severityLevel;
    private RetryQueueItemStatus _status;

    public RetryQueueItemBuilder(RetryQueueBuilder retryQueueBuilder, int sort)
    {
        Guard.Argument(retryQueueBuilder, nameof(retryQueueBuilder)).NotNull();

        _retryQueueBuilder = retryQueueBuilder;

        // defaults
        _attemptsCount = 0;
        _creationDate = RetryQueueBuilder.DefaultDateTime;
        _sort = sort;
        _lastExecution = RetryQueueBuilder.DefaultDateTime;
        _modifiedStatusDate = RetryQueueBuilder.DefaultDateTime;
        _status = RetryQueueItemStatus.Waiting;
        _severityLevel = SeverityLevel.Medium;
        _description = string.Empty;
        _message = DefaultItemMessage;
    }

    public RetryQueueBuilder AddItem()
    {
        return _retryQueueBuilder.WithItem(Build());
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
        _modifiedStatusDate = modifiedStatusDate;

        return this;
    }

    public RetryQueueItemBuilder WithSeverityLevel(SeverityLevel severityLevel)
    {
        _severityLevel = severityLevel;

        return this;
    }

    public RetryQueueItemBuilder WithStatus(RetryQueueItemStatus status)
    {
        _status = status;

        return this;
    }

    public RetryQueueItemBuilder WithWaitingStatus()
    {
        return WithStatus(RetryQueueItemStatus.Waiting);
    }

    private RetryQueueItem Build()
    {
        _id = _id == default ? Guid.NewGuid() : _id;

        return new RetryQueueItem(
            _id,
            _attemptsCount,
            _creationDate,
            _sort,
            _lastExecution,
            _modifiedStatusDate,
            _status,
            _severityLevel,
            _description)
        {
            Message = _message
        };
    }
}