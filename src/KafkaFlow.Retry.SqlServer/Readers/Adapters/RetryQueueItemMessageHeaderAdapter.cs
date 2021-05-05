namespace KafkaFlow.Retry.SqlServer.Readers.Adapters
{
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.SqlServer.Model;

    internal class RetryQueueItemMessageHeaderAdapter : IRetryQueueItemMessageHeaderAdapter
    {
        public MessageHeader Adapt(RetryQueueItemMessageHeaderDbo messageHeaderDbo)
        {
            Guard.Argument(messageHeaderDbo).NotNull();

            return new MessageHeader(messageHeaderDbo.Key, messageHeaderDbo.Value);
        }
    }
}
