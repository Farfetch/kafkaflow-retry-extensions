using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

public interface IRepository
{
    RepositoryType RepositoryType { get; }

    IRetryDurableQueueRepositoryProvider RetryQueueDataProvider { get; }

    Task CleanDatabaseAsync();

    Task CreateQueueAsync(RetryQueue queue);

    Task<RetryQueue> GetAllRetryQueueDataAsync(string queueGroupKey);

    Task<RetryQueue> GetRetryQueueAsync(string queueGroupKey);

    Task<IList<RetryQueueItem>> GetRetryQueueItemsAsync(Guid retryQueueId, Func<IList<RetryQueueItem>, bool> stopCondition);
}