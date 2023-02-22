namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Polling;
    using Moq;
    using Xunit;

    public class QueueTrackerFactoryTests
    {
        public static IEnumerable<object[]> DataTest() => new List<object[]>
        {
            new object[]
            {
                null,
                Mock.Of<IJobDataProvidersFactory>(),
                typeof(ArgumentNullException)
            },
            new object[]
            {
                string.Empty,
                Mock.Of<IJobDataProvidersFactory>(),
                typeof(ArgumentException)
            },
            new object[]
            {
                "id",
                null,
                typeof(ArgumentNullException)
            }
        };

        [Fact]
        public void QueueTrackerFactory_Create_Success()
        {
            // Arrange
            var mockJobDataProvidersFactory = new Mock<IJobDataProvidersFactory>();
            mockJobDataProvidersFactory
                .Setup(m => m.Create(It.IsAny<IMessageProducer>(), It.IsAny<ILogHandler>()))
                .Returns(new[] { Mock.Of<IJobDataProvider>() });

            var factory = new QueueTrackerFactory("id", mockJobDataProvidersFactory.Object);

            // Act
            var queueTracker = factory.Create(Mock.Of<IMessageProducer>(), Mock.Of<ILogHandler>());

            // Arrange
            queueTracker.Should().NotBeNull();
        }

        [Theory]
        [MemberData(nameof(DataTest))]
        internal void QueueTrackerFactory_Ctor_WithArgumentNull_ThrowsException(
            string schedulerId,
            IJobDataProvidersFactory jobDataProvidersFactory,
            Type expectedExceptionType)
        {
            // Act & Assert
            Assert.Throws(expectedExceptionType, () => new QueueTrackerFactory(schedulerId, jobDataProvidersFactory));
        }
    }
}