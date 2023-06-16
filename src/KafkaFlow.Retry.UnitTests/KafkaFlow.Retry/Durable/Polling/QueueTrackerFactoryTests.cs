﻿namespace KafkaFlow.Retry.UnitTests.KafkaFlow.Retry.Durable.Polling
{
    using System;
    using System.Collections.Generic;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Definitions;
    using global::KafkaFlow.Retry.Durable.Encoders;
    using global::KafkaFlow.Retry.Durable.Polling;
    using global::KafkaFlow.Retry.Durable.Repository;
    using global::KafkaFlow.Retry.Durable.Repository.Adapters;
    using Moq;
    using Xunit;

    public class QueueTrackerFactoryTests
    {
        public static readonly IEnumerable<object[]> DataTest = new List<object[]>
        {
            new object[]
            {
                null,
                Mock.Of<IMessageHeadersAdapter>() ,
                Mock.Of<IMessageAdapter>() ,
                Mock.Of<IUtf8Encoder>() ,
            },
            new object[]
            {
                Mock.Of<IRetryDurableQueueRepository>(),
                null ,
                Mock.Of<IMessageAdapter>() ,
                Mock.Of<IUtf8Encoder>()
            },
            new object[]
            {
                Mock.Of<IRetryDurableQueueRepository>(),
                Mock.Of<IMessageHeadersAdapter>() ,
                null ,
                Mock.Of<IUtf8Encoder>()
            },
            new object[]
            {
                Mock.Of<IRetryDurableQueueRepository>(),
                Mock.Of<IMessageHeadersAdapter>() ,
                Mock.Of<IMessageAdapter>() ,
                null
            }
        };

        private static readonly RetryDurablePollingDefinition retryDurablePollingDefinition = new RetryDurablePollingDefinition(true, "*/30 * * ? * *", 10, 100, "id");

        [Fact]
        public void QueueTrackerFactory_Create_Success()
        {
            // Arrange
            var factory = new QueueTrackerFactory(
                Mock.Of<IRetryDurableQueueRepository>(),
                Mock.Of<IMessageHeadersAdapter>(),
                Mock.Of<IMessageAdapter>(),
                Mock.Of<IUtf8Encoder>());

            // Act
            var queueTracker = factory.Create(retryDurablePollingDefinition, Mock.Of<IMessageProducer>(), Mock.Of<ILogHandler>());

            // Arrange
            queueTracker.Should().NotBeNull();
        }

        [Theory]
        [MemberData(nameof(DataTest))]
        public void QueueTrackerFactory_Ctor_WithArgumentNull_ThrowsException(
            object retryDurableQueueRepository,
            object messageHeadersAdapter,
            object messageAdapter,
            object utf8Encoder)
        {
            // Arrange & Act
            Action act = () => new QueueTrackerFactory(
            (IRetryDurableQueueRepository)retryDurableQueueRepository,
            (IMessageHeadersAdapter)messageHeadersAdapter,
            (IMessageAdapter)messageAdapter,
            (IUtf8Encoder)utf8Encoder);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}