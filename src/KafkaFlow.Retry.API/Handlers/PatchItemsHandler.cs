using System;
using System.Net;
using System.Threading.Tasks;
using KafkaFlow.Retry.API.Adapters.UpdateItems;
using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.Durable.Repository;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace KafkaFlow.Retry.API.Handlers;

internal class PatchItemsHandler : RetryRequestHandlerBase
{
    private readonly IRetryDurableQueueRepositoryProvider _retryDurableQueueRepositoryProvider;
    private readonly IUpdateItemsInputAdapter _updateItemsInputAdapter;
    private readonly IUpdateItemsResponseDtoAdapter _updateItemsResponseDtoAdapter;

    public PatchItemsHandler(
        IRetryDurableQueueRepositoryProvider retryDurableQueueRepositoryProvider,
        IUpdateItemsInputAdapter updateItemsInputAdapter,
        IUpdateItemsResponseDtoAdapter updateItemsResponseDtoAdapter,
        string endpointPrefix) : base(endpointPrefix, "items")
    {
        _retryDurableQueueRepositoryProvider = retryDurableQueueRepositoryProvider;
        _updateItemsInputAdapter = updateItemsInputAdapter;
        _updateItemsResponseDtoAdapter = updateItemsResponseDtoAdapter;
    }

    protected override HttpMethod HttpMethod => HttpMethod.PATCH;

    protected override async Task HandleRequestAsync(HttpRequest request, HttpResponse response)
    {
        UpdateItemsRequestDto requestDto;

        try
        {
            requestDto = await ReadRequestDtoAsync<UpdateItemsRequestDto>(request).ConfigureAwait(false);
        }
        catch (JsonSerializationException ex)
        {
            await WriteResponseAsync(response, ex, (int)HttpStatusCode.BadRequest).ConfigureAwait(false);

            return;
        }
        catch (Exception ex)
        {
            await WriteResponseAsync(response, ex, (int)HttpStatusCode.InternalServerError).ConfigureAwait(false);

            return;
        }

        try
        {
            var input = _updateItemsInputAdapter.Adapt(requestDto);

            var result = await _retryDurableQueueRepositoryProvider.UpdateItemsAsync(input).ConfigureAwait(false);

            var responseDto = _updateItemsResponseDtoAdapter.Adapt(result);

            await WriteResponseAsync(response, responseDto, (int)HttpStatusCode.OK).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await WriteResponseAsync(response, ex, (int)HttpStatusCode.InternalServerError).ConfigureAwait(false);
        }
    }
}