namespace KafkaFlow.Retry.UnitTests.Repositories.MongoDb.Adapters
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using FluentAssertions;
    using global::KafkaFlow.Retry.Durable.Repository.Model;
    using global::KafkaFlow.Retry.MongoDb.Adapters;
    using global::KafkaFlow.Retry.MongoDb.Model;
    using Xunit;

    [ExcludeFromCodeCoverage]
    public class HeaderAdapterTests
    {
        [Fact]
        public void HeaderAdapter_Adapt_WithMessageHeader_Success()
        {
            //Arrange
            var adapter = new HeaderAdapter();
            var message = new MessageHeader("key", new byte[2]);

            // Act
            var result = adapter.Adapt(message);

            // Assert
            adapter.Should().NotBeNull();
            result.Should().BeOfType(typeof(RetryQueueHeaderDbo));
        }

        [Fact]
        public void HeaderAdapter_Adapt_WithoutMessageHeader_ThrowException()
        {
            //Arrange
            var adapter = new HeaderAdapter();
            MessageHeader message = null;

            // Act
            Action act = () => adapter.Adapt(message);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}