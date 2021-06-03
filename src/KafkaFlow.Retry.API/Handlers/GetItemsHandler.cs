namespace KafkaFlow.Retry.API.Handlers
{
    using System.Net;
    using System.Threading.Tasks;
    using Dawn;
    using KafkaFlow.Retry.API.Adapters.GetItems;
    using KafkaFlow.Retry.Durable.Repository;
    using Microsoft.AspNetCore.Http;

    internal class GetItemsHandler : RetryRequestHandlerBase
    {
        private readonly IGetItemsInputAdapter getItemsInputAdapter;
        private readonly IGetItemsRequestDtoReader getItemsRequestDtoReader;
        private readonly IGetItemsResponseDtoAdapter getItemsResponseDtoAdapter;
        private readonly IKafkaRetryDurableQueueRepositoryProvider kafkaRetryDurableQueueRepositoryProvider;

        public GetItemsHandler(
            IKafkaRetryDurableQueueRepositoryProvider kafkaRetryDurableQueueRepositoryProvider,
            IGetItemsRequestDtoReader getItemsRequestDtoReader,
            IGetItemsInputAdapter getItemsInputAdapter,
            IGetItemsResponseDtoAdapter getItemsResponseDtoAdapter)
        {
            Guard.Argument(kafkaRetryDurableQueueRepositoryProvider, nameof(kafkaRetryDurableQueueRepositoryProvider)).NotNull();
            Guard.Argument(getItemsRequestDtoReader, nameof(getItemsRequestDtoReader)).NotNull();
            Guard.Argument(getItemsInputAdapter, nameof(getItemsInputAdapter)).NotNull();
            Guard.Argument(getItemsResponseDtoAdapter, nameof(getItemsResponseDtoAdapter)).NotNull();

            this.getItemsInputAdapter = getItemsInputAdapter;
            this.kafkaRetryDurableQueueRepositoryProvider = kafkaRetryDurableQueueRepositoryProvider;
            this.getItemsRequestDtoReader = getItemsRequestDtoReader;
            this.getItemsResponseDtoAdapter = getItemsResponseDtoAdapter;
        }

        protected override string ResourcePath => base.ResourcePath.ExtendResourcePath("items");

        protected override HttpMethod HttpMethod => HttpMethod.GET;

        protected override async Task HandleRequestAsync(HttpRequest request, HttpResponse response)
        {
            try
            {
                var requestDto = this.getItemsRequestDtoReader.Read(request);

                var input = this.getItemsInputAdapter.Adapt(requestDto);

                var result = await this.kafkaRetryDurableQueueRepositoryProvider.GetQueuesAsync(input).ConfigureAwait(false);

                var responseDto = this.getItemsResponseDtoAdapter.Adapt(result);

                await this.WriteResponseAsync(response, responseDto, (int)HttpStatusCode.OK).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                await this.WriteResponseAsync(response, ex, (int)HttpStatusCode.InternalServerError).ConfigureAwait(false);
            }
        }
    }
}