namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable
{
    using global::KafkaFlow.Configuration;
    using global::KafkaFlow.Retry.Durable;
    using Moq;
    using Xunit;

    public class ConfigurationBuilderExtensionsTests
    {
        [Fact]
        public void ConfigurationBuilderExtensions_RetryConsumerStrategy_ForGuaranteeOrderedConsumption_ReturnsRetryDurableConsumerGuaranteeOrderedMiddleware()
        {
            // Arrange
            var consumerMiddleware = new Mock<IConsumerMiddlewareConfigurationBuilder>();

            // Act
            consumerMiddleware.Object.RetryConsumerStrategy(RetryConsumerStrategy.GuaranteeOrderedConsumption);

            // Assert
            consumerMiddleware.Verify(d => d.Add(It.IsAny<Factory<RetryDurableConsumerGuaranteeOrderedMiddleware>>()), Times.Once);
        }

        [Fact]
        public void ConfigurationBuilderExtensions_RetryConsumerStrategy_ForLatestConsumption_ReturnsRetryDurableConsumerLatestMiddleware()
        {
            // Arrange
            var consumerMiddleware = new Mock<IConsumerMiddlewareConfigurationBuilder>();

            // Act
            consumerMiddleware.Object.RetryConsumerStrategy(RetryConsumerStrategy.LatestConsumption);

            // Assert
            consumerMiddleware.Verify(d => d.Add(It.IsAny<Factory<RetryDurableConsumerLatestMiddleware>>()), Times.Once);
        }
    }
}