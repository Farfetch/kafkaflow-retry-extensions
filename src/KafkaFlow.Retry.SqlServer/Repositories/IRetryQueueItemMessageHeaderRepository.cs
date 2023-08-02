namespace KafkaFlow.Retry.SqlServer.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.SqlServer.Model;

    internal interface IRetryQueueItemMessageHeaderRepository
    {
        Task AddAsync(IDbConnection dbConnection, IEnumerable<RetryQueueItemMessageHeaderDbo> retryQueueHeadersDbo, string schema);

        Task<IList<RetryQueueItemMessageHeaderDbo>> GetOrderedAsync(IDbConnection dbConnection, IEnumerable<RetryQueueItemMessageDbo> retryQueueItemMessagesDbo, string schema);
    }
}
