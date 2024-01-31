﻿using System;
using System.Diagnostics.CodeAnalysis;
using Dawn;
using KafkaFlow.Retry.Durable.Common;

namespace KafkaFlow.Retry.Durable.Repository.Model;

[ExcludeFromCodeCoverage]
public class RetryQueueItem
{
    public RetryQueueItem(
        Guid id,
        int attemptsCount,
        DateTime creationDate,
        int sort,
        DateTime? lastExecution,
        DateTime? modifiedStatusDate,
        RetryQueueItemStatus status,
        SeverityLevel severityLevel,
        string description)
    {
        Guard.Argument(id).NotDefault();
        Guard.Argument(attemptsCount).NotNegative();
        Guard.Argument(creationDate).NotDefault();
        Guard.Argument(sort).NotNegative();
        Guard.Argument(status).NotDefault();

        Id = id;
        AttemptsCount = attemptsCount;
        CreationDate = creationDate;
        Sort = sort;
        LastExecution = lastExecution;
        ModifiedStatusDate = modifiedStatusDate;
        Status = status;
        SeverityLevel = severityLevel;
        Description = description;
    }

    public int AttemptsCount { get; }
    public DateTime CreationDate { get; }

    public string Description { get; }

    public Guid Id { get; }

    public DateTime? LastExecution { get; }

    public DateTime? ModifiedStatusDate { get; }

    public RetryQueueItemMessage Message { get; set; }

    public SeverityLevel SeverityLevel { get; }

    public int Sort { get; }

    public RetryQueueItemStatus Status { get; }
}