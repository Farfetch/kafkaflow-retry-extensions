using System;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Actions.Create;
using KafkaFlow.Retry.MongoDb.Adapters.Interfaces;

namespace KafkaFlow.Retry.MongoDb.Model.Factories;

internal class RetryQueueItemDboFactory
{
    private readonly IMessageAdapter _messageAdapter;

    public RetryQueueItemDboFactory(IMessageAdapter messageAdapter)
    {
        _messageAdapter = messageAdapter;
    }

    public RetryQueueItemDbo Create(SaveToQueueInput input, Guid queueId, int sort = 0)
    {
        Guard.Argument(input, nameof(input)).NotNull();
        Guard.Argument(queueId).NotDefault();
        Guard.Argument(sort, nameof(sort)).NotNegative();

        return new RetryQueueItemDbo
        {
            CreationDate = input.CreationDate,
            LastExecution = input.LastExecution,
            ModifiedStatusDate = input.ModifiedStatusDate,
            AttemptsCount = input.AttemptsCount,
            Message = _messageAdapter.Adapt(input.Message),
            RetryQueueId = queueId,
            Sort = sort,
            Status = input.ItemStatus,
            SeverityLevel = input.SeverityLevel,
            Description = input.Description
        };
    }
}