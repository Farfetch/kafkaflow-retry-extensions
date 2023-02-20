using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using KafkaFlow.Retry.Durable.Common;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.Postgres.Model;

namespace KafkaFlow.Retry.Postgres.Repositories
{
    internal interface IRetryQueueItemRepository
    {
        Task<long> AddAsync(IDbConnection dbConnection, RetryQueueItemDbo retryQueueItemDbo);

        Task<bool> AnyItemStillActiveAsync(IDbConnection dbConnection, Guid retryQueueId);

        Task<RetryQueueItemDbo> GetItemAsync(IDbConnection dbConnection, Guid domainId);

        Task<IList<RetryQueueItemDbo>> GetItemsByQueueOrderedAsync(IDbConnection dbConnection, Guid retryQueueId);

        Task<IList<RetryQueueItemDbo>> GetItemsOrderedAsync(
            IDbConnection dbConnection,
            IEnumerable<Guid> retryQueueIds,
            IEnumerable<RetryQueueItemStatus> statuses,
            IEnumerable<SeverityLevel> severities = null,
            int? top = null,
            StuckStatusFilter stuckStatusFilter = null);

        Task<IList<RetryQueueItemDbo>> GetNewestItemsAsync(IDbConnection dbConnection, Guid queueIdDomain, int sort);

        Task<IList<RetryQueueItemDbo>> GetPendingItemsAsync(IDbConnection dbConnection, Guid queueIdDomain, int sort);

        Task<bool> IsFirstWaitingInQueueAsync(IDbConnection dbConnection, RetryQueueItemDbo item);

        Task<int> UpdateAsync(IDbConnection dbConnection, Guid idDomain, RetryQueueItemStatus status, int attemptsCount, DateTime lastExecution, string description);

        Task<int> UpdateStatusAsync(IDbConnection dbConnection, Guid idDomain, RetryQueueItemStatus status);
    }
}