namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry
{
    using System;
    using global::KafkaFlow.Configuration;
    using global::KafkaFlow.Retry.Durable;
    using global::KafkaFlow.Retry.Forever;
    using global::KafkaFlow.Retry.Simple;
    using Moq;
    using Xunit;

    public class ConfigurationBuilderExtensionsTests
    {
        [Fact(Skip = "I will discuss with the team")]
        public void ConfigurationBuilderExtensions_RetryDurable_ReturnsRetryDurableMiddleware()
        {
            // Arrange
            var consumerMiddleware = new Mock<IConsumerMiddlewareConfigurationBuilder>();
            

            // Act
            consumerMiddleware.Object.RetryDurable(null);

            // Assert
            consumerMiddleware.Verify(d => d.Add(It.IsAny<Factory<RetryDurableMiddleware>>()), Times.Once);
        }

        [Fact(Skip = "I will discuss with the team")]
        public void ConfigurationBuilderExtensions_RetrySimple_ReturnsRetryForeverMiddleware()
        {
            // Arrange
            var consumerMiddleware = new Mock<IConsumerMiddlewareConfigurationBuilder>();


            // Act
            consumerMiddleware.Object.RetryDurable(null);

            // Assert
            consumerMiddleware.Verify(d => d.Add(It.IsAny<Factory<RetryForeverMiddleware>>()), Times.Once);
        }

        [Fact(Skip = "I will discuss with the team")]
        public void ConfigurationBuilderExtensions_RetryForever_ReturnsRetrySimpleMiddleware()
        {
            // Arrange
            var consumerMiddleware = new Mock<IConsumerMiddlewareConfigurationBuilder>();


            // Act
            consumerMiddleware.Object.RetryDurable(null);

            // Assert
            consumerMiddleware.Verify(d => d.Add(It.IsAny<Factory<RetrySimpleMiddleware>>()), Times.Once);
        }
    }
}