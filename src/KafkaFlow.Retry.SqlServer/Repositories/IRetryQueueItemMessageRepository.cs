namespace KafkaFlow.Retry.SqlServer.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.SqlServer.Model;

    internal interface IRetryQueueItemMessageRepository
    {
        Task AddAsync(IDbConnection dbConnection, RetryQueueItemMessageDbo retryQueueItemMessageDbo, string schema);

        Task<IList<RetryQueueItemMessageDbo>> GetMessagesOrderedAsync(IDbConnection dbConnection, IEnumerable<RetryQueueItemDbo> retryQueueItemsDbo, string schema);
    }
}