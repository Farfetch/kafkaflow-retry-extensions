namespace KafkaFlow.Retry.IntegrationTests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoFixture;
    using KafkaFlow.Retry.IntegrationTests.Core;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.Retry.IntegrationTests.Core.Producers;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    [Collection("BootstrapperHostCollection")]
    public class RetrySimpleTests
    {
        private readonly BootstrapperHostFixture bootstrapperHostFixture;
        private readonly Fixture fixture = new Fixture();
        private IServiceProvider provider;

        public RetrySimpleTests(BootstrapperHostFixture bootstrapperHostFixture)
        {
            this.bootstrapperHostFixture = bootstrapperHostFixture;
            InMemoryAuxiliarStorage.Clear();
        }

        [Fact]
        public async Task RetrySimpleTest()
        {
            // Arrange
            var producer1 = this.bootstrapperHostFixture.ServiceProvider.GetRequiredService<IMessageProducer<RetrySimpleProducer>>();
            var messages = this.fixture.CreateMany<RetrySimpleTestMessage>(10).ToList();

            // Act
            messages.ForEach(m => producer1.Produce(m.Key, m));

            // Assert
            foreach (var message in messages)
            {
                await InMemoryAuxiliarStorage.AssertCountRetrySimpleMessageAsync(message, 4);
            }
        }
    }
}