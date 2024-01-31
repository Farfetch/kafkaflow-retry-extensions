using System;
using System.Net;
using System.Threading.Tasks;
using Dawn;
using KafkaFlow.Retry.API.Adapters.GetItems;
using KafkaFlow.Retry.Durable.Repository;
using Microsoft.AspNetCore.Http;

namespace KafkaFlow.Retry.API.Handlers;

internal class GetItemsHandler : RetryRequestHandlerBase
{
    private readonly IGetItemsInputAdapter _getItemsInputAdapter;
    private readonly IGetItemsRequestDtoReader _getItemsRequestDtoReader;
    private readonly IGetItemsResponseDtoAdapter _getItemsResponseDtoAdapter;
    private readonly IRetryDurableQueueRepositoryProvider _retryDurableQueueRepositoryProvider;

    public GetItemsHandler(
        IRetryDurableQueueRepositoryProvider retryDurableQueueRepositoryProvider,
        IGetItemsRequestDtoReader getItemsRequestDtoReader,
        IGetItemsInputAdapter getItemsInputAdapter,
        IGetItemsResponseDtoAdapter getItemsResponseDtoAdapter,
        string endpointPrefix) : base(endpointPrefix, "items")
    {
        Guard.Argument(retryDurableQueueRepositoryProvider, nameof(retryDurableQueueRepositoryProvider)).NotNull();
        Guard.Argument(getItemsRequestDtoReader, nameof(getItemsRequestDtoReader)).NotNull();
        Guard.Argument(getItemsInputAdapter, nameof(getItemsInputAdapter)).NotNull();
        Guard.Argument(getItemsResponseDtoAdapter, nameof(getItemsResponseDtoAdapter)).NotNull();

        _getItemsInputAdapter = getItemsInputAdapter;
        _retryDurableQueueRepositoryProvider = retryDurableQueueRepositoryProvider;
        _getItemsRequestDtoReader = getItemsRequestDtoReader;
        _getItemsResponseDtoAdapter = getItemsResponseDtoAdapter;
    }

    protected override HttpMethod HttpMethod => HttpMethod.GET;

    protected override async Task HandleRequestAsync(HttpRequest request, HttpResponse response)
    {
        try
        {
            var requestDto = _getItemsRequestDtoReader.Read(request);

            var input = _getItemsInputAdapter.Adapt(requestDto);

            var result = await _retryDurableQueueRepositoryProvider.GetQueuesAsync(input).ConfigureAwait(false);

            var responseDto = _getItemsResponseDtoAdapter.Adapt(result);

            await WriteResponseAsync(response, responseDto, (int)HttpStatusCode.OK).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await WriteResponseAsync(response, ex, (int)HttpStatusCode.InternalServerError).ConfigureAwait(false);
        }
    }
}