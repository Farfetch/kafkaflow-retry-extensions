namespace KafkaFlow.Retry.API.Handlers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.API.Adapters.UpdateItems;
    using KafkaFlow.Retry.API.Dtos;
    using KafkaFlow.Retry.Durable.Repository;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    internal class PatchItemsHandler : RetryRequestHandlerBase
    {
        private readonly IRetryDurableQueueRepositoryProvider retryDurableQueueRepositoryProvider;
        private readonly IUpdateItemsInputAdapter updateItemsInputAdapter;
        private readonly IUpdateItemsResponseDtoAdapter updateItemsResponseDtoAdapter;

        public PatchItemsHandler(
            IRetryDurableQueueRepositoryProvider retryDurableQueueRepositoryProvider,
            IUpdateItemsInputAdapter updateItemsInputAdapter,
            IUpdateItemsResponseDtoAdapter updateItemsResponseDtoAdapter)
        {
            this.retryDurableQueueRepositoryProvider = retryDurableQueueRepositoryProvider;
            this.updateItemsInputAdapter = updateItemsInputAdapter;
            this.updateItemsResponseDtoAdapter = updateItemsResponseDtoAdapter;
        }

        protected override string ResourcePath => base.ResourcePath.ExtendResourcePath("items");

        protected override HttpMethod HttpMethod => HttpMethod.PATCH;

        protected override async Task HandleRequestAsync(HttpRequest request, HttpResponse response)
        {
            UpdateItemsRequestDto requestDto;

            try
            {
                requestDto = await this.ReadRequestDtoAsync<UpdateItemsRequestDto>(request).ConfigureAwait(false);
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
                var input = this.updateItemsInputAdapter.Adapt(requestDto);

                var result = await this.retryDurableQueueRepositoryProvider.UpdateItemsAsync(input).ConfigureAwait(false);

                var responseDto = this.updateItemsResponseDtoAdapter.Adapt(result);

                await this.WriteResponseAsync(response, responseDto, (int)HttpStatusCode.OK).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await this.WriteResponseAsync(response, ex, (int)HttpStatusCode.InternalServerError).ConfigureAwait(false);
            }
        }
    }
}