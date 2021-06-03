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
            this IApplicationBuilder appBuilder
        )
        {
            var kafkaRetryDurableQueueRepositoryProvider =
                appBuilder
                    .ApplicationServices
                    .GetService(typeof(IKafkaRetryDurableQueueRepositoryProvider)) as IKafkaRetryDurableQueueRepositoryProvider;

            appBuilder.UseMiddleware<RetryMiddleware>(
                new GetItemsHandler(
                    kafkaRetryDurableQueueRepositoryProvider,
                    new GetItemsRequestDtoReader(),
                    new GetItemsInputAdapter(),
                    new GetItemsResponseDtoAdapter()));

            appBuilder.UseMiddleware<RetryMiddleware>(
                new PatchItemsHandler(
                    kafkaRetryDurableQueueRepositoryProvider,
                    new UpdateItemsInputAdapter(),
                    new UpdateItemsResponseDtoAdapter()));

            appBuilder.UseMiddleware<RetryMiddleware>(
                new PatchQueuesHandler(
                    kafkaRetryDurableQueueRepositoryProvider,
                    new UpdateQueuesInputAdapter(),
                    new UpdateQueuesResponseDtoAdapter()));

            return appBuilder;
        }

        public static IApplicationBuilder UseRetryEndpoints(
                    this IApplicationBuilder appBuilder,
            IKafkaRetryDurableQueueRepositoryProvider kafkaRetryDurableQueueRepositoryProvider
        )
        {
            appBuilder.UseMiddleware<RetryMiddleware>(
                new GetItemsHandler(
                    kafkaRetryDurableQueueRepositoryProvider,
                    new GetItemsRequestDtoReader(),
                    new GetItemsInputAdapter(),
                    new GetItemsResponseDtoAdapter()));

            appBuilder.UseMiddleware<RetryMiddleware>(
                new PatchItemsHandler(
                    kafkaRetryDurableQueueRepositoryProvider,
                    new UpdateItemsInputAdapter(),
                    new UpdateItemsResponseDtoAdapter()));

            appBuilder.UseMiddleware<RetryMiddleware>(
                new PatchQueuesHandler(
                    kafkaRetryDurableQueueRepositoryProvider,
                    new UpdateQueuesInputAdapter(),
                    new UpdateQueuesResponseDtoAdapter()));

            return appBuilder;
        }
    }
}