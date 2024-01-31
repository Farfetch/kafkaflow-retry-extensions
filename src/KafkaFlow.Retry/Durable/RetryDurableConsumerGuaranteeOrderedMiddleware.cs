using System;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Durable.Encoders;
using KafkaFlow.Retry.Durable.Repository;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;
using KafkaFlow.Retry.Durable.Repository.Model;

namespace KafkaFlow.Retry.Durable;

internal class RetryDurableConsumerGuaranteeOrderedMiddleware : IMessageMiddleware
{
    private readonly ILogHandler _logHandler;
    private readonly IRetryDurableQueueRepository _retryDurableQueueRepository;
    private readonly IUtf8Encoder _utf8Encoder;

    public RetryDurableConsumerGuaranteeOrderedMiddleware(
        ILogHandler logHandler,
        IRetryDurableQueueRepository retryDurableQueueRepository,
        IUtf8Encoder utf8Encoder)
    {
        Guard.Argument(logHandler).NotNull();
        Guard.Argument(retryDurableQueueRepository).NotNull();
        Guard.Argument(utf8Encoder).NotNull();

        _logHandler = logHandler;
        _retryDurableQueueRepository = retryDurableQueueRepository;
        _utf8Encoder = utf8Encoder;
    }

    public async Task Invoke(IMessageContext context, MiddlewareDelegate next)
    {
        var queueId = Guid.Parse(_utf8Encoder.Decode(context.Headers[RetryDurableConstants.QueueId]));
        var itemId = Guid.Parse(_utf8Encoder.Decode(context.Headers[RetryDurableConstants.ItemId]));
        var attemptsCount = int.Parse(_utf8Encoder.Decode(context.Headers[RetryDurableConstants.AttemptsCount]));
        var sort = int.Parse(_utf8Encoder.Decode(context.Headers[RetryDurableConstants.Sort]));
        var pendingItems = false;
        try
        {
            pendingItems = await ThereArePendingItemsAsync(
                    queueId,
                    itemId,
                    sort)
                .ConfigureAwait(false);

            if (pendingItems)
            {
                await UpdateAsync(
                    RetryQueueItemStatus.Waiting,
                    queueId,
                    itemId,
                    attemptsCount
                ).ConfigureAwait(false);

                return;
            }

            await next(context).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            await UpdateAsync(
                RetryQueueItemStatus.Waiting,
                queueId,
                itemId,
                attemptsCount,
                exception
            ).ConfigureAwait(false);
        }
    }

    private async Task<bool> ThereArePendingItemsAsync(
        Guid queueId,
        Guid itemId,
        int sort)
    {
        var queuePendingItemsInput = new
            QueuePendingItemsInput(
                queueId,
                itemId,
                sort
            );

        var queuePendingItemsResult = await _retryDurableQueueRepository
            .CheckQueuePendingItemsAsync(queuePendingItemsInput)
            .ConfigureAwait(false);

        return queuePendingItemsResult.Status == QueuePendingItemsResultStatus.HasPendingItems;
    }

    private async Task UpdateAsync(
        RetryQueueItemStatus targetStatus,
        Guid queueId,
        Guid itemId,
        int attemptsCount,
        Exception exception = null)
    {
        await _retryDurableQueueRepository
            .UpdateItemAsync(
                new UpdateItemExecutionInfoInput(
                    queueId,
                    itemId,
                    targetStatus,
                    attemptsCount,
                    DateTime.UtcNow,
                    exception?.ToString()
                )
            )
            .ConfigureAwait(false);
    }
}