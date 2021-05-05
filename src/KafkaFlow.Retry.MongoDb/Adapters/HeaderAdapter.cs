namespace KafkaFlow.Retry.MongoDb.Adapters
{
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.MongoDb.Adapters.Interfaces;
    using KafkaFlow.Retry.MongoDb.Model;

    internal class HeaderAdapter : IHeaderAdapter
    {
        public RetryQueueHeaderDbo Adapt(MessageHeader header)
        {
            Guard.Argument(header, nameof(header)).NotNull();

            return new RetryQueueHeaderDbo
            {
                Key = header.Key,
                Value = header.Value
            };
        }

        public MessageHeader Adapt(RetryQueueHeaderDbo headerDbo)
        {
            Guard.Argument(headerDbo, nameof(headerDbo)).NotNull();

            return new MessageHeader(headerDbo.Key, headerDbo.Value);
        }
    }
}