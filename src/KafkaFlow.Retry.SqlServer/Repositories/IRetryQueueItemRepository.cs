namespace KafkaFlow.Retry.SqlServer.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.SqlServer.Model;

    internal interface IRetryQueueItemRepository
    {
        Task<long> AddAsync(IDbConnection dbConnection, RetryQueueItemDbo retryQueueItemDbo, string schema);

        Task<bool> AnyItemStillActiveAsync(IDbConnection dbConnection, Guid retryQueueId, string schema);

        Task<RetryQueueItemDbo> GetItemAsync(IDbConnection dbConnection, Guid domainId, string schema);

        Task<IList<RetryQueueItemDbo>> GetItemsByQueueOrderedAsync(IDbConnection dbConnection, Guid retryQueueId, string schema);

        Task<IList<RetryQueueItemDbo>> GetItemsOrderedAsync(
            IDbConnection dbConnection,
            IEnumerable<Guid> retryQueueIds,
            IEnumerable<RetryQueueItemStatus> statuses,
            string schema,
            IEnumerable<SeverityLevel> severities = null,
            int? top = null,
            StuckStatusFilter stuckStatusFilter = null);

        Task<IList<RetryQueueItemDbo>> GetNewestItemsAsync(IDbConnection dbConnection, Guid queueIdDomain, int sort, string schema);

        Task<IList<RetryQueueItemDbo>> GetPendingItemsAsync(IDbConnection dbConnection, Guid queueIdDomain, int sort, string schema);

        Task<bool> IsFirstWaitingInQueueAsync(IDbConnection dbConnection, RetryQueueItemDbo item, string schema);

        Task<int> UpdateAsync(IDbConnection dbConnection, Guid idDomain, RetryQueueItemStatus status, int attemptsCount, DateTime lastExecution, string description, string schema);

        Task<int> UpdateStatusAsync(IDbConnection dbConnection, Guid idDomain, RetryQueueItemStatus status, string schema);
    }
}
