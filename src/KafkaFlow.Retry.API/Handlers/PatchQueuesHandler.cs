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
    private readonly IRetryDurableQueueRepositoryProvider retryDurableQueueRepositoryProvider;
    private readonly IUpdateQueuesInputAdapter updateQueuesInputAdapter;
    private readonly IUpdateQueuesResponseDtoAdapter updateQueuesResponseDtoAdapter;

    public PatchQueuesHandler(
        IRetryDurableQueueRepositoryProvider retryDurableQueueRepositoryProvider,
        IUpdateQueuesInputAdapter updateQueuesInputAdapter,
        IUpdateQueuesResponseDtoAdapter updateQueuesResponseDtoAdapter,
        string endpointPrefix) : base(endpointPrefix, "queues")
    {
        this.retryDurableQueueRepositoryProvider = retryDurableQueueRepositoryProvider;
        this.updateQueuesInputAdapter = updateQueuesInputAdapter;
        this.updateQueuesResponseDtoAdapter = updateQueuesResponseDtoAdapter;
    }


    protected override HttpMethod HttpMethod => HttpMethod.PATCH;

    protected override async Task HandleRequestAsync(HttpRequest request, HttpResponse response)
    {
        UpdateQueuesRequestDto requestDto;

        try
        {
            requestDto = await this.ReadRequestDtoAsync<UpdateQueuesRequestDto>(request).ConfigureAwait(false);
        }
        catch (JsonSerializationException ex)
        {
            await this.WriteResponseAsync(response, ex, (int)HttpStatusCode.BadRequest).ConfigureAwait(false);

            return;
        }
        catch (Exception ex)
        {
            await this.WriteResponseAsync(response, ex, (int)HttpStatusCode.InternalServerError).ConfigureAwait(false);

            return;
        }

        try
        {
            var input = this.updateQueuesInputAdapter.Adapt(requestDto);

            var result = await this.retryDurableQueueRepositoryProvider.UpdateQueuesAsync(input).ConfigureAwait(false);

            var responseDto = this.updateQueuesResponseDtoAdapter.Adapt(result);

            await this.WriteResponseAsync(response, responseDto, (int)HttpStatusCode.OK).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await this.WriteResponseAsync(response, ex, (int)HttpStatusCode.InternalServerError).ConfigureAwait(false);
        }
    }
}