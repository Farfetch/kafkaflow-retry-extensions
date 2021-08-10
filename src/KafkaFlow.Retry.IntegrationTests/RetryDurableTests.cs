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

    public class RetryDurableTests : IDisposable
    {
        private readonly Fixture fixture = new Fixture();
        private IServiceProvider provider;

        public RetryDurableTests()
        {
            this.provider = Bootstrapper.GetServiceProvider();
            MessageStorage.Clear();
            MessageStorage.ThrowException = true;
            MongoStorage.DropDatabase();
        }

        public void Dispose()
        {
            Bootstrapper.KafkaBusStop();
        }

        [Fact]
        public async Task RetryDurableTest_ConsumerStrategyGuaranteeOrderedConsumption()
        {
            // Arrange
            var numberOfMessages = 5;
            var numberOfMessagesByEachSameKey = 10;
            var numberOfTimesThatEachMessageIsTriedBeforeDurable = 4;
            var numberOfTimesThatEachMessageIsTriedDuringDurable = 2;
            var numberOfTimesThatEachMessageIsTriedWhenDone = numberOfMessagesByEachSameKey;
            var producer = this.provider.GetRequiredService<IMessageProducer<RetryDurableProducer>>();
            var messages = this.fixture.CreateMany<RetryDurableTestMessage>(numberOfMessages).ToList();

            // Act
            messages.ForEach(
                m =>
                {
                    for (int i = 0; i < numberOfMessagesByEachSameKey; i++)
                    {
                        producer.Produce(m.Key, m);
                    }
                });

            // Assert - Creation
            foreach (var message in messages)
            {
                await MessageStorage.AssertCountRetryDurableMessageAsync(message, numberOfTimesThatEachMessageIsTriedBeforeDurable).ConfigureAwait(false);
            }

            foreach (var message in messages)
            {
                await MongoStorage.AssertRetryDurableMessageCreationAsync(message, numberOfMessagesByEachSameKey).ConfigureAwait(false);
            }

            // Assert - Retrying
            MessageStorage.Clear();

            foreach (var message in messages)
            {
                await MessageStorage.AssertCountRetryDurableMessageAsync(message, numberOfTimesThatEachMessageIsTriedDuringDurable).ConfigureAwait(false);
            }

            foreach (var message in messages)
            {
                await MongoStorage.AssertRetryDurableMessageRetryingAsync(message, numberOfTimesThatEachMessageIsTriedDuringDurable).ConfigureAwait(false);
            }

            // Assert - Done
            MessageStorage.ThrowException = false;
            MessageStorage.Clear();

            foreach (var message in messages)
            {
                await MessageStorage.AssertCountRetryDurableMessageAsync(message, numberOfTimesThatEachMessageIsTriedWhenDone).ConfigureAwait(false);
            }

            foreach (var message in messages)
            {
                await MongoStorage.AssertRetryDurableMessageDoneAsync(message).ConfigureAwait(false);
            }
        }
    }
}