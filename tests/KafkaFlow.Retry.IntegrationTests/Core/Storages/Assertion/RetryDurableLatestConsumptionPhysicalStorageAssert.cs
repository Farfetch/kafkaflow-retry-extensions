﻿using System;
using System.Linq;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.IntegrationTests.Core.Messages;
using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Assertion;

internal class RetryDurableLatestConsumptionPhysicalStorageAssert : IPhysicalStorageAssert
{
    private readonly IRepositoryProvider _repositoryProvider;

    public RetryDurableLatestConsumptionPhysicalStorageAssert(IRepositoryProvider repositoryProvider)
    {
        _repositoryProvider = repositoryProvider;
    }

    public async Task AssertRetryDurableMessageCreationAsync(RepositoryType repositoryType,
        RetryDurableTestMessage message, int count)
    {
        var retryQueue = await _repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueAsync(message.Key)
            ;

        Assert.True(retryQueue.Id != Guid.Empty, "Retry Durable Creation Get Retry Queue cannot be asserted.");

        var retryQueueItems = await _repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueItemsAsync(retryQueue.Id, rqi => { return rqi.Count() != count; })
            ;

        Assert.True(retryQueueItems != null, "Retry Durable Creation Get Retry Queue Item Message cannot be asserted.");

        Assert.Equal(0, retryQueueItems.Sum(i => i.AttemptsCount));
        Assert.Equal(retryQueueItems.Count() - 1, retryQueueItems.Max(i => i.Sort));
        Assert.True(Equals(retryQueue.Status, RetryQueueStatus.Active));
        Assert.All(retryQueueItems, i => Equals(i.Status, RetryQueueItemStatus.Waiting));
    }

    public async Task AssertRetryDurableMessageDoneAsync(RepositoryType repositoryType, RetryDurableTestMessage message)
    {
        var retryQueue = await _repositoryProvider
                .GetRepositoryOfType(repositoryType)
                .GetRetryQueueAsync(message.Key)
            ;

        Assert.True(retryQueue.Id != Guid.Empty, "Retry Durable Done Get Retry Queue cannot be asserted.");

        var retryQueueItems = await _repositoryProvider
            .GetRepositoryOfType(repositoryType)
            .GetRetryQueueItemsAsync(
                retryQueue.Id,
                rqi => { return rqi.OrderBy(x => x.Sort).Last().Status != RetryQueueItemStatus.Done; });

        Assert.True(retryQueueItems != null, "Retry Durable Done Get Retry Queue Item Message cannot be asserted.");

        Assert.True(Equals(retryQueue.Status, RetryQueueStatus.Done));
    }

    public async Task AssertRetryDurableMessageRetryingAsync(RepositoryType repositoryType,
        RetryDurableTestMessage message, int retryCount)
    {
        var retryQueue = await _repositoryProvider
            .GetRepositoryOfType(repositoryType)
            .GetRetryQueueAsync(message.Key);

        Assert.True(retryQueue.Id != Guid.Empty, "Retry Durable Retrying Get Retry Queue cannot be asserted.");

        var retryQueueItems = await _repositoryProvider
            .GetRepositoryOfType(repositoryType)
            .GetRetryQueueItemsAsync(
                retryQueue.Id,
                rqi => { return rqi.OrderBy(x => x.Sort).Last().AttemptsCount != retryCount; });

        Assert.True(retryQueueItems != null, "Retry Durable Retrying Get Retry Queue Item Message cannot be asserted.");

        Assert.True(Equals(retryQueue.Status, RetryQueueStatus.Active));
        Assert.Equal(retryQueueItems.Count() - 1, retryQueueItems.Max(i => i.Sort));
        Assert.Equal(RetryQueueItemStatus.Waiting, retryQueueItems.OrderBy(x => x.Sort).Last().Status);
        Assert.All(retryQueueItems.OrderByDescending(x => x.Sort).Skip(1),
            i => Equals(i.Status, RetryQueueItemStatus.Cancelled));
    }
}