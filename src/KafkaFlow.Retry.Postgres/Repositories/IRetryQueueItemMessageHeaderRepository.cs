namespace KafkaFlow.Retry.Postgres.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Postgres.Model;
    
    internal interface IRetryQueueItemMessageHeaderRepository
    {
        Task AddAsync(IDbConnection dbConnection, IEnumerable<RetryQueueItemMessageHeaderDbo> retryQueueHeadersDbo);

        Task<IList<RetryQueueItemMessageHeaderDbo>> GetOrderedAsync(IDbConnection dbConnection, IEnumerable<RetryQueueItemMessageDbo> retryQueueItemMessagesDbo);
    }
}
