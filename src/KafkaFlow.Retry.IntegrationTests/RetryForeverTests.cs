namespace KafkaFlow.Retry.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using KafkaFlow.Retry.IntegrationTests.Core;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.Retry.IntegrationTests.Core.Producers;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    public class RetryForeverTests
    {
        private readonly Fixture fixture = new Fixture();
        private IServiceProvider provider;

        public RetryForeverTests()
        {
            this.provider = Bootstrapper.GetServiceProvider();
            MessageStorage.Clear();
        }

        [Fact]
        public async Task RetryForeverTest()
        {
            // Arrange
            var producer1 = this.provider.GetRequiredService<IMessageProducer<RetryForeverProducer>>();
            var messages = this.fixture.CreateMany<RetryForeverTestMessage>(1).ToList();

            // Act
            messages.ForEach(m => producer1.Produce(m.Key, m));

            // Assert
            foreach (var message in messages)
            {
                await MessageStorage.AssertCountRetryForeverMessageAsync(message, 20);
            }
        }
    }
}