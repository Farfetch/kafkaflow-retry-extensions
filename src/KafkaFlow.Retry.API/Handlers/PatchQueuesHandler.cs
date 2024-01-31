using System;
using System.Net;
using System.Threading.Tasks;
using KafkaFlow.Retry.API.Adapters.UpdateQueues;
using KafkaFlow.Retry.API.Dtos;
using KafkaFlow.Retry.Durable.Repository;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace KafkaFlow.Retry.API.Handlers;

internal class PatchQueuesHandler : RetryRequestHandlerBase
{
    private readonly IRetryDurableQueueRepositoryProvider _retryDurableQueueRepositoryProvider;
    private readonly IUpdateQueuesInputAdapter _updateQueuesInputAdapter;
    private readonly IUpdateQueuesResponseDtoAdapter _updateQueuesResponseDtoAdapter;

    public PatchQueuesHandler(
        IRetryDurableQueueRepositoryProvider retryDurableQueueRepositoryProvider,
        IUpdateQueuesInputAdapter updateQueuesInputAdapter,
        IUpdateQueuesResponseDtoAdapter updateQueuesResponseDtoAdapter,
        string endpointPrefix) : base(endpointPrefix, "queues")
    {
        _retryDurableQueueRepositoryProvider = retryDurableQueueRepositoryProvider;
        _updateQueuesInputAdapter = updateQueuesInputAdapter;
        _updateQueuesResponseDtoAdapter = updateQueuesResponseDtoAdapter;
    }


    protected override HttpMethod HttpMethod => HttpMethod.PATCH;

    protected override async Task HandleRequestAsync(HttpRequest request, HttpResponse response)
    {
        UpdateQueuesRequestDto requestDto;

        try
        {
            requestDto = await ReadRequestDtoAsync<UpdateQueuesRequestDto>(request).ConfigureAwait(false);
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
            var input = _updateQueuesInputAdapter.Adapt(requestDto);

            var result = await _retryDurableQueueRepositoryProvider.UpdateQueuesAsync(input).ConfigureAwait(false);

            var responseDto = _updateQueuesResponseDtoAdapter.Adapt(result);

            await WriteResponseAsync(response, responseDto, (int)HttpStatusCode.OK).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await WriteResponseAsync(response, ex, (int)HttpStatusCode.InternalServerError).ConfigureAwait(false);
        }
    }
}