namespace KafkaFlow.Retry.SqlServer.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.SqlServer.Model;

    internal interface IRetryQueueRepository
    {
        Task<long> AddAsync(IDbConnection dbConnection, RetryQueueDbo retryQueueDbo, string schema);

        Task<int> DeleteQueuesAsync(IDbConnection dbConnection, string searchGroupKey, RetryQueueStatus retryQueueStatus, DateTime maxLastExecutionDateToBeKept, int maxRowsToDelete, string schema);

        Task<bool> ExistsActiveAsync(IDbConnection dbConnection, string queueGroupKey, string schema);

        Task<RetryQueueDbo> GetQueueAsync(IDbConnection dbConnection, string queueGroupKey, string schema);

        Task<IList<RetryQueueDbo>> GetTopSortedQueuesOrderedAsync(IDbConnection dbConnection, RetryQueueStatus retryQueueStatus, GetQueuesSortOption sortOption, string searchGroupKey, int top, string schema);

        Task<int> UpdateAsync(IDbConnection dbConnection, Guid idDomain, RetryQueueStatus retryQueueStatus, DateTime lastExecution, string schema);

        Task<int> UpdateLastExecutionAsync(IDbConnection dbConnection, Guid idDomain, DateTime lastExecution, string schema);

        Task<int> UpdateStatusAsync(IDbConnection dbConnection, Guid idDomain, RetryQueueStatus retryQueueStatus, string schema);
    }
}
