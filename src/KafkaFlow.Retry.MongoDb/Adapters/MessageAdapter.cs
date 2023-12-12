using System.Linq;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Adapters.Interfaces;
using KafkaFlow.Retry.MongoDb.Model;

namespace KafkaFlow.Retry.MongoDb.Adapters;

internal class MessageAdapter : IMessageAdapter
{
    private readonly IHeaderAdapter headerAdapter;

    public MessageAdapter(IHeaderAdapter headerAdapter)
    {
            Guard.Argument(headerAdapter, nameof(headerAdapter)).NotNull();

            this.headerAdapter = headerAdapter;
        }

    public RetryQueueItemMessage Adapt(RetryQueueItemMessageDbo messageDbo)
    {
            Guard.Argument(messageDbo, nameof(messageDbo)).NotNull();

            return new RetryQueueItemMessage(
                messageDbo.TopicName,
                messageDbo.Key,
                messageDbo.Value,
                messageDbo.Partition,
                messageDbo.Offset,
                messageDbo.UtcTimeStamp,
                messageDbo.Headers?.Select(headerDbo => this.headerAdapter.Adapt(headerDbo)));
        }

    public RetryQueueItemMessageDbo Adapt(RetryQueueItemMessage message)
    {
            Guard.Argument(message, nameof(message)).NotNull();

            return new RetryQueueItemMessageDbo
            {
                Key = message.Key,
                Value = message.Value,
                Offset = message.Offset,
                Partition = message.Partition,
                TopicName = message.TopicName,
                UtcTimeStamp = message.UtcTimeStamp,
                Headers = message.Headers.Select(h => this.headerAdapter.Adapt(h))
            };
        }
}