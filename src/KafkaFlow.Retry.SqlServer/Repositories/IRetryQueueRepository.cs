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
        Task<long> AddAsync(IDbConnection dbConnection, RetryQueueDbo retryQueueDbo);

        Task<bool> ExistsActiveAsync(IDbConnection dbConnection, string queueGroupKey);

        Task<RetryQueueDbo> GetQueueAsync(IDbConnection dbConnection, string queueGroupKey);

        Task<IList<RetryQueueDbo>> GetTopSortedQueuesOrderedAsync(IDbConnection dbConnection, RetryQueueStatus retryQueueStatus, GetQueuesSortOption sortOption, string searchGroupKey, int top);

        Task<int> UpdateAsync(IDbConnection dbConnection, Guid idDomain, RetryQueueStatus retryQueueStatus, DateTime lastExecution);

        Task<int> UpdateLastExecutionAsync(IDbConnection dbConnection, Guid idDomain, DateTime lastExecution);

        Task<int> UpdateStatusAsync(IDbConnection dbConnection, Guid idDomain, RetryQueueStatus retryQueueStatus);
    }
}
