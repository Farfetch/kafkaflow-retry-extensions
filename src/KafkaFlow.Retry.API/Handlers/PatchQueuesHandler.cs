namespace KafkaFlow.Retry.API.Handlers
{
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.API.Adapters.UpdateQueues;
    using KafkaFlow.Retry.API.Dtos;
    using KafkaFlow.Retry.Durable.Repository;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    internal class PatchQueuesHandler : RetryRequestHandlerBase
    {
        private readonly IKafkaRetryDurableQueueRepositoryProvider kafkaRetryDurableQueueRepositoryProvider;
        private readonly IUpdateQueuesInputAdapter updateQueuesInputAdapter;
        private readonly IUpdateQueuesResponseDtoAdapter updateQueuesResponseDtoAdapter;

        public PatchQueuesHandler(
            IKafkaRetryDurableQueueRepositoryProvider kafkaRetryDurableQueueRepositoryProvider,
            IUpdateQueuesInputAdapter updateQueuesInputAdapter,
            IUpdateQueuesResponseDtoAdapter updateQueuesResponseDtoAdapter)
        {
            this.kafkaRetryDurableQueueRepositoryProvider = kafkaRetryDurableQueueRepositoryProvider;
            this.updateQueuesInputAdapter = updateQueuesInputAdapter;
            this.updateQueuesResponseDtoAdapter = updateQueuesResponseDtoAdapter;
        }

        protected override string ResourcePath => base.ResourcePath.ExtendResourcePath("queues");

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

                var result = await this.kafkaRetryDurableQueueRepositoryProvider.UpdateQueuesAsync(input).ConfigureAwait(false);

                var responseDto = this.updateQueuesResponseDtoAdapter.Adapt(result);

                await this.WriteResponseAsync(response, responseDto, (int)HttpStatusCode.OK).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                await this.WriteResponseAsync(response, ex, (int)HttpStatusCode.InternalServerError).ConfigureAwait(false);
            }
        }
    }
}