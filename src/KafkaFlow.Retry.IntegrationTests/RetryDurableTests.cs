namespace KafkaFlow.Retry.IntegrationTests
{
    using System;
    using System.Collections.Generic;
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
    public class RetryDurableTests
    {
        private readonly BootstrapperHostFixture bootstrapperHostFixture;
        private readonly Fixture fixture = new Fixture();

        public RetryDurableTests(BootstrapperHostFixture bootstrapperHostFixture)
        {
            this.bootstrapperHostFixture = bootstrapperHostFixture;

            MessageStorage.Clear();
            MessageStorage.ThrowException = true;
        }

        public static IEnumerable<object[]> GetStorages()
        {
            yield return new object[] { typeof(MongoStorage), typeof(IMessageProducer<RetryDurableGuaranteeOrderedConsumptionMongoDbProducer>) };
            yield return new object[] { typeof(SqlServerStorage), typeof(IMessageProducer<RetryDurableGuaranteeOrderedConsumptionSqlServerProducer>) };
        }

        [Theory]
        [MemberData(nameof(GetStorages))]
        internal async Task RetryDurableTest_ConsumerStrategyGuaranteeOrderedConsumption(Type storageType, Type producerType)
        {
            // Arrange
            var numberOfMessages = 5;
            var numberOfMessagesByEachSameKey = 10;
            var numberOfTimesThatEachMessageIsTriedBeforeDurable = 4;
            var numberOfTimesThatEachMessageIsTriedDuringDurable = 2;
            var numberOfTimesThatEachMessageIsTriedWhenDone = numberOfMessagesByEachSameKey;
            var storage = this.bootstrapperHostFixture.ServiceProvider.GetRequiredService(storageType) as IStorage;
            var producer = this.bootstrapperHostFixture.ServiceProvider.GetRequiredService(producerType) as IMessageProducer;
            var messages = this.fixture.CreateMany<RetryDurableTestMessage>(numberOfMessages).ToList();
            storage.CleanDatabase();

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
                await storage.AssertRetryDurableMessageCreationAsync(message, numberOfMessagesByEachSameKey).ConfigureAwait(false);
            }

            // Assert - Retrying
            MessageStorage.Clear();

            foreach (var message in messages)
            {
                await MessageStorage.AssertCountRetryDurableMessageAsync(message, numberOfTimesThatEachMessageIsTriedDuringDurable).ConfigureAwait(false);
            }

            foreach (var message in messages)
            {
                await storage.AssertRetryDurableMessageRetryingAsync(message, numberOfTimesThatEachMessageIsTriedDuringDurable).ConfigureAwait(false);
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
                await storage.AssertRetryDurableMessageDoneAsync(message).ConfigureAwait(false);
            }
        }
    }
}