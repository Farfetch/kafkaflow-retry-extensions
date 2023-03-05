namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling
{
    using System;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Definitions.Polling;
    using global::KafkaFlow.Retry.Durable.Encoders;
    using global::KafkaFlow.Retry.Durable.Polling;
    using global::KafkaFlow.Retry.Durable.Repository;
    using global::KafkaFlow.Retry.Durable.Repository.Adapters;
    using Moq;
    using Quartz;
    using Xunit;

    public class JobDataProvidersFactoryTests
    {
        private static readonly PollingDefinitionsAggregator pollingDefinitionsAggregator =
            new PollingDefinitionsAggregator(
                "id",
                new PollingDefinition[]
                {
                    new RetryDurablePollingDefinition(true, "*/30 * * ? * *", 10, 100),
                    new CleanupPollingDefinition(true, "*/30 * * ? * *", 10, 100)
                }
            );

        [Fact]
        public void JobDataProvidersFactory_Create_Success()
        {
            // Arrange
            var mockTriggerProvider = new Mock<ITriggerProvider>();
            mockTriggerProvider
                .Setup(m => m.GetPollingTrigger(It.IsAny<string>(), It.IsAny<PollingDefinition>()))
                .Returns(Mock.Of<ITrigger>());

            var factory = new JobDataProvidersFactory(
                pollingDefinitionsAggregator,
                mockTriggerProvider.Object,
                Mock.Of<IRetryDurableQueueRepository>(),
                Mock.Of<IMessageHeadersAdapter>(),
                Mock.Of<IUtf8Encoder>());

            // Act
            var jobDataProviders = factory.Create(Mock.Of<IMessageProducer>(), Mock.Of<ILogHandler>());

            // Arrange
            jobDataProviders.Should().NotBeNull();
        }

        [Theory]
        [InlineData(typeof(PollingDefinitionsAggregator))]
        [InlineData(typeof(ITriggerProvider))]
        [InlineData(typeof(IRetryDurableQueueRepository))]
        [InlineData(typeof(IMessageHeadersAdapter))]
        [InlineData(typeof(IUtf8Encoder))]
        public void JobDataProvidersFactory_Ctor_WithArgumentNull_ThrowsException(Type nullType)
        {
            // Arrange & Act
            Action act = () => new JobDataProvidersFactory(
                nullType == typeof(PollingDefinitionsAggregator) ? null : pollingDefinitionsAggregator,
                nullType == typeof(ITriggerProvider) ? null : Mock.Of<ITriggerProvider>(),
                nullType == typeof(IRetryDurableQueueRepository) ? null : Mock.Of<IRetryDurableQueueRepository>(),
                nullType == typeof(IMessageHeadersAdapter) ? null : Mock.Of<IMessageHeadersAdapter>(),
                nullType == typeof(IUtf8Encoder) ? null : Mock.Of<IUtf8Encoder>());
            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}