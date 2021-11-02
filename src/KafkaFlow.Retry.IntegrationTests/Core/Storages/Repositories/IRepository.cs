namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Models;

    internal interface IRepository
    {
        Type RepositoryType { get; }

        Task CleanDatabaseAsync();

        Task<RetryQueueTestModel> GetRetryQueueAsync(RetryDurableTestMessage message);

        Task<IList<RetryQueueItemTestModel>> GetRetryQueueItemsAsync(
           Guid retryQueueId,
           Func<IList<RetryQueueItemTestModel>, bool> stopCondition);
    }
}