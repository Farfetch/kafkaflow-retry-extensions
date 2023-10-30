namespace KafkaFlow.Retry.API
{
    using KafkaFlow.Retry.API.Adapters.GetItems;
    using KafkaFlow.Retry.API.Adapters.UpdateItems;
    using KafkaFlow.Retry.API.Adapters.UpdateQueues;
    using KafkaFlow.Retry.API.Handlers;
    using KafkaFlow.Retry.Durable.Repository;
    using Microsoft.AspNetCore.Builder;

    public static class AppBuilderExtensions
    {

        public static IApplicationBuilder UseKafkaFlowRetryEndpoints(
           this IApplicationBuilder appBuilder,
           string endpointPrefix)
        {
            var retryDurableQueueRepositoryProvider =
                appBuilder
                    .ApplicationServices
                    .GetService(typeof(IRetryDurableQueueRepositoryProvider)) as IRetryDurableQueueRepositoryProvider;

            appBuilder.UseRetryEndpoints(retryDurableQueueRepositoryProvider, endpointPrefix);

            return appBuilder;
        }

        public static IApplicationBuilder UseKafkaFlowRetryEndpoints(
        this IApplicationBuilder appBuilder)
        {
            return appBuilder.UseKafkaFlowRetryEndpoints(string.Empty);
        }

        public static IApplicationBuilder UseRetryEndpoints(
                    this IApplicationBuilder appBuilder,
            IRetryDurableQueueRepositoryProvider retryDurableQueueRepositoryProvider,
            string endpointPrefix
        )
        {
            appBuilder.UseMiddleware<RetryMiddleware>(
                new GetItemsHandler(
                    retryDurableQueueRepositoryProvider,
                    new GetItemsRequestDtoReader(),
                    new GetItemsInputAdapter(),
                    new GetItemsResponseDtoAdapter(),
                    endpointPrefix));

            appBuilder.UseMiddleware<RetryMiddleware>(
                new PatchItemsHandler(
                    retryDurableQueueRepositoryProvider,
                    new UpdateItemsInputAdapter(),
                    new UpdateItemsResponseDtoAdapter(),
                    endpointPrefix));

            appBuilder.UseMiddleware<RetryMiddleware>(
                new PatchQueuesHandler(
                    retryDurableQueueRepositoryProvider,
                    new UpdateQueuesInputAdapter(),
                    new UpdateQueuesResponseDtoAdapter(),
                    endpointPrefix));

            return appBuilder;
        }

        public static IApplicationBuilder UseRetryEndpoints(
                    this IApplicationBuilder appBuilder,
            IRetryDurableQueueRepositoryProvider retryDurableQueueRepositoryProvider
        )
        {
            return appBuilder.UseRetryEndpoints(retryDurableQueueRepositoryProvider, string.Empty);
        }

    }
}