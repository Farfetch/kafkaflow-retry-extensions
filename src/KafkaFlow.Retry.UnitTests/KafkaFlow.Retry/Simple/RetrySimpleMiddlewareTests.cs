namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Simple
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using global::KafkaFlow.Retry.Simple;
    using Moq;
    using Xunit;

    public class RetrySimpleMiddlewareTests
    {
        [Fact(Skip = "needs to be constructed")]
        public async Task RetrySimpleMiddleware_Ctor_Tests()
        {
            //Arrange
            Mock<ILogHandler> mockILogHandler = new Mock<ILogHandler>();
            var retrySimpleDefinition = new RetrySimpleDefinition(1, Mock.Of<IReadOnlyCollection<Func<RetryContext, bool>>>(), false, (a) => { return TimeSpan.FromSeconds(1); });

            var retrySimpleMiddleware = new RetrySimpleMiddleware(
                mockILogHandler.Object,
                retrySimpleDefinition
                );

            Mock<IMessageContext> mockIMessageContext = new Mock<IMessageContext>();

            //Act
            await retrySimpleMiddleware.Invoke(mockIMessageContext.Object, null);

            // Assert
            Assert.True(true);
        }
    }
}