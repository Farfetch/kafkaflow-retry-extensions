using Dawn;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.MongoDb.Adapters.Interfaces;
using KafkaFlow.Retry.MongoDb.Model;

namespace KafkaFlow.Retry.MongoDb.Adapters;

internal class ItemAdapter : IItemAdapter
{
    private readonly IMessageAdapter messageAdapter;

    public ItemAdapter(IMessageAdapter messageAdater)
    {
            Guard.Argument(messageAdater, nameof(messageAdater)).NotNull();

            this.messageAdapter = messageAdater;
        }

    public RetryQueueItem Adapt(RetryQueueItemDbo itemDbo)
    {
            Guard.Argument(itemDbo, nameof(itemDbo)).NotNull();

            return new RetryQueueItem(
                itemDbo.Id,
                itemDbo.AttemptsCount,
                itemDbo.CreationDate,
                itemDbo.Sort,
                itemDbo.LastExecution,
                itemDbo.ModifiedStatusDate,
                itemDbo.Status,
                itemDbo.SeverityLevel,
                itemDbo.Description
            )
            {
                Message = this.messageAdapter.Adapt(itemDbo.Message)
            };
        }
}