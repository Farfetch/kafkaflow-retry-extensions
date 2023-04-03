namespace KafkaFlow.Retry.Postgres.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.Postgres.Model;
    
    internal interface IRetryQueueItemMessageRepository
    {
        Task AddAsync(IDbConnection dbConnection, RetryQueueItemMessageDbo retryQueueItemMessageDbo);

        Task<IList<RetryQueueItemMessageDbo>> GetMessagesOrderedAsync(IDbConnection dbConnection, IEnumerable<RetryQueueItemDbo> retryQueueItemsDbo);
    }
}
