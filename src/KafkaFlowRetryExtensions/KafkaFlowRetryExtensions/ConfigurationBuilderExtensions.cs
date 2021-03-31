namespace KafkaFlow.Retry
{
    using KafkaFlow;
    using KafkaFlow.Configuration;

    public static class ConfigurationBuilderExtensions
    {
        /// <summary>
        /// Registers a middleware to decompress the message
        /// </summary>
        public static IConsumerMiddlewareConfigurationBuilder AddRetryPolicy<T>(this IConsumerMiddlewareConfigurationBuilder middlewares)
            where T : class, ILogHandler
        {
            middlewares.DependencyConfigurator.AddTransient<T>();
            return middlewares.AddRetryPolicy(resolver => resolver.Resolve<T>());
        }

        /// <summary>
        /// Registers a middleware to decompress the message
        /// </summary>
        /// <param name="middlewares"></param>
        /// <param name="factory">A factory to create the <see cref="IMessageCompressor"/> instance</param>
        /// <returns></returns>
        public static IConsumerMiddlewareConfigurationBuilder AddRetryPolicy<T>(
            this IConsumerMiddlewareConfigurationBuilder middlewares,
            Factory<T> factory)
            where T : class, ILogHandler
        {
            return middlewares.Add(resolver => new KafkaRetryMiddleware(factory(resolver)));
        }
    }
}