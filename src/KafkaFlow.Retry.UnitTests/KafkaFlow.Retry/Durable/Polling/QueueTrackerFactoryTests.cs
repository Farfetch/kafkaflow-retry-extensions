using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using global::KafkaFlow.Retry.Durable.Definitions.Polling;
using global::KafkaFlow.Retry.Durable.Encoders;
using global::KafkaFlow.Retry.Durable.Polling;
using global::KafkaFlow.Retry.Durable.Repository;
using global::KafkaFlow.Retry.Durable.Repository.Adapters;
using Moq;
using Xunit;

namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling;

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

    [Fact]
    public async Task QueueTrackerFactory_Reschedule_Success()
    {
            var mockILogHandler = new Mock<ILogHandler>();
            mockILogHandler.Setup(x => x.Info(It.IsAny<string>(), It.IsAny<object>()));
            mockILogHandler.Setup(x => x.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<object>()));

            var mockIMessageProducer = new Mock<IMessageProducer>();

            var pollingDefinitionsAggregator =
                new PollingDefinitionsAggregator(
                    "topic",
                    new List<PollingDefinition>
                    {
                        new CleanupPollingDefinition(true, "*/5 * * ? * * *",1,1),
                        new RetryDurablePollingDefinition(true, "*/5 * * ? * * *",1,1)
                    });

            var queueTrackerCoordinator =
                new QueueTrackerCoordinator(
                    new QueueTrackerFactory(
                        pollingDefinitionsAggregator.SchedulerId,
                        new JobDataProvidersFactory(
                            pollingDefinitionsAggregator,
                            new TriggerProvider(),
                            new NullRetryDurableQueueRepository(),
                            new MessageHeadersAdapter(),
                            new Utf8Encoder()
                        )
                    )
                );

            await queueTrackerCoordinator.ScheduleJobsAsync(mockIMessageProducer.Object, mockILogHandler.Object).ConfigureAwait(false);
            await queueTrackerCoordinator.UnscheduleJobsAsync().ConfigureAwait(false);
            await queueTrackerCoordinator.ScheduleJobsAsync(mockIMessageProducer.Object, mockILogHandler.Object).ConfigureAwait(false);

            mockILogHandler.Verify(x => x.Error(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<object>()), Times.Never);
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