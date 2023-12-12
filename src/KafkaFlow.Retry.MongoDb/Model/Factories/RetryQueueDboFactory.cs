using Dawn;
using KafkaFlow.Retry.Durable.Repository.Actions.Create;

namespace KafkaFlow.Retry.MongoDb.Model.Factories;

internal static class RetryQueueDboFactory
{
    internal static RetryQueueDbo Create(SaveToQueueInput input)
    {
            Guard.Argument(input).NotNull();

            return new RetryQueueDbo
            {
                SearchGroupKey = input.SearchGroupKey,
                QueueGroupKey = input.QueueGroupKey,
                CreationDate = input.CreationDate,
                LastExecution = input.LastExecution.Value,
                Status = input.QueueStatus
            };
        }
}