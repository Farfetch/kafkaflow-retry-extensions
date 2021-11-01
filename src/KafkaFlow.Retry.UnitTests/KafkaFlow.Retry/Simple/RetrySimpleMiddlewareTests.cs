namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Simple
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using System.Threading.Tasks;
    using global::KafkaFlow.Retry.Simple;
    using Moq;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class RetrySimpleMiddlewareTests
    {
        [Fact]
        public async Task RetrySimpleMiddleware_Invoke_Successfully()
        {
            //Arrange
            string expectedConsumerName = "ConsumerName";
            Mock<ILogHandler> mockILogHandler = new Mock<ILogHandler>();
            var retrySimpleDefinition = new RetrySimpleDefinition(1, Mock.Of<IReadOnlyCollection<Func<RetryContext, bool>>>(), false, (a) => { return TimeSpan.FromSeconds(1); });

            var retrySimpleMiddleware = new RetrySimpleMiddleware(
                mockILogHandler.Object,
                retrySimpleDefinition
                );

            Mock<IConsumerContext> mockIConsumerContext = new Mock<IConsumerContext>();
            mockIConsumerContext
                .SetupGet(ctx => ctx.WorkerId)
                .Returns(1);
            mockIConsumerContext
                .SetupGet(ctx => ctx.ConsumerName)
                .Returns(expectedConsumerName);
            mockIConsumerContext
                .SetupGet(ctx => ctx.GroupId)
                .Returns("GroupId");
            mockIConsumerContext
                .SetupGet(ctx => ctx.Partition)
                .Returns(2);
            mockIConsumerContext
                .SetupGet(ctx => ctx.WorkerStopped)
                .Returns(CancellationToken.None);

            Mock<IMessageContext> mockIMessageContext = new Mock<IMessageContext>();
            mockIMessageContext
                .Setup(ctx => ctx.ConsumerContext)
                .Returns(mockIConsumerContext.Object);

            string actualConsumerName = null;
            MiddlewareDelegate middlewareDelegate = delegate (IMessageContext context)
            {
                Console.WriteLine($"Notification received for: {context.Message.Key}");
                actualConsumerName = context.ConsumerContext.ConsumerName;
                return Task.CompletedTask;
            };

            //Act
            await retrySimpleMiddleware.Invoke(mockIMessageContext.Object, middlewareDelegate);

            // Assert
            Assert.Equal(expectedConsumerName, actualConsumerName);
        }
    }
}