﻿using System;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.Durable.Repository.Actions.Update;

namespace KafkaFlow.Retry.Durable.Repository;

internal class UpdateRetryQueueItemExecutionInfoHandler : IUpdateRetryQueueItemHandler
{
    private readonly IRetryDurableQueueRepositoryProvider _retryDurableQueueRepositoryProvider;

    public UpdateRetryQueueItemExecutionInfoHandler(
        IRetryDurableQueueRepositoryProvider retryDurableQueueRepositoryProvider)
    {
        Guard.Argument(retryDurableQueueRepositoryProvider).NotNull();

        _retryDurableQueueRepositoryProvider = retryDurableQueueRepositoryProvider;
    }

    public bool CanHandle(UpdateItemInput input)
    {
        return input is UpdateItemExecutionInfoInput;
    }

    public async Task UpdateItemAsync(UpdateItemInput input)
    {
        Guard.Argument(input, nameof(input)).Compatible<UpdateItemExecutionInfoInput>(i =>
            $"The input have to be a {nameof(UpdateItemExecutionInfoInput)}.");

        var updateItemExecutionInfoInput = input as UpdateItemExecutionInfoInput;

        try
        {
            var result = await _retryDurableQueueRepositoryProvider
                .UpdateItemExecutionInfoAsync(updateItemExecutionInfoInput).ConfigureAwait(false);

            if (result.Status != UpdateItemResultStatus.Updated)
            {
                var kafkaException = new RetryDurableException(
                    new RetryError(RetryErrorCode.DataProviderUpdateItem),
                    $"{result.Status} while updating the item execution info."
                );

                kafkaException.Data.Add(nameof(updateItemExecutionInfoInput.QueueId),
                    updateItemExecutionInfoInput.QueueId);
                kafkaException.Data.Add(nameof(updateItemExecutionInfoInput.ItemId),
                    updateItemExecutionInfoInput.ItemId);
                kafkaException.Data.Add(nameof(updateItemExecutionInfoInput.Status),
                    updateItemExecutionInfoInput.Status);

                throw kafkaException;
            }
        }
        catch (Exception ex) when (!(ex is RetryDurableException))
        {
            var kafkaException = new RetryDurableException(
                new RetryError(RetryErrorCode.DataProviderUpdateItem),
                "An error ocurred while trying to update the item execution info.", ex
            );

            kafkaException.Data.Add(nameof(updateItemExecutionInfoInput.QueueId), updateItemExecutionInfoInput.QueueId);
            kafkaException.Data.Add(nameof(updateItemExecutionInfoInput.ItemId), updateItemExecutionInfoInput.ItemId);
            kafkaException.Data.Add(nameof(updateItemExecutionInfoInput.Status), updateItemExecutionInfoInput.Status);

            throw kafkaException;
        }
    }
}