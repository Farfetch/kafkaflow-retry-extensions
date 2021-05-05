namespace KafkaFlow.Retry.SqlServer.Readers.Adapters
{
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.SqlServer.Model;

    internal class RetryQueueAdapter : IRetryQueueAdapter
    {
        public RetryQueue Adapt(RetryQueueDbo retryQueueDbo)
        {
            Guard.Argument(retryQueueDbo).NotNull();

            return new RetryQueue(retryQueueDbo.IdDomain,
                        retryQueueDbo.SearchGroupKey,
                        retryQueueDbo.QueueGroupKey,
                        retryQueueDbo.CreationDate,
                        retryQueueDbo.LastExecution,
                        retryQueueDbo.Status);
        }
    }
}
