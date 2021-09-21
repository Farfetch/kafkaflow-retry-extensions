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
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    [Collection("BootstrapperHostCollection")]
    public class RetryDurableTests
    {
        private readonly BootstrapperHostFixture bootstrapperHostFixture;
        private readonly Fixture fixture = new Fixture();
        private readonly IRepositoryProvider repositoryProvider;
        private readonly IServiceProvider serviceProvider;

        public RetryDurableTests(BootstrapperHostFixture bootstrapperHostFixture)
        {
            this.serviceProvider = bootstrapperHostFixture.ServiceProvider;
            this.repositoryProvider = bootstrapperHostFixture.ServiceProvider.GetRequiredService<IRepositoryProvider>();
            InMemoryAuxiliarStorage.Clear();
            InMemoryAuxiliarStorage.ThrowException = true;
        }

        public static IEnumerable<object[]> Scenarios()
        {
            yield return new object[]
            {
                typeof(MongoDbRepository),
                typeof(IMessageProducer<RetryDurableGuaranteeOrderedConsumptionMongoDbProducer>),
                typeof(RetryDurableGuaranteeOrderedConsumptionPhysicalStorageAssert),
                10
            };
            yield return new object[]
            {
                typeof(SqlServerRepository),
                typeof(IMessageProducer<RetryDurableGuaranteeOrderedConsumptionSqlServerProducer>),
                typeof(RetryDurableGuaranteeOrderedConsumptionPhysicalStorageAssert),
                10
            };
            yield return new object[]
            {
                typeof(MongoDbRepository),
                typeof(IMessageProducer<RetryDurableLatestConsumptionMongoDbProducer>),
                typeof(RetryDurableLatestConsumptionPhysicalStorageAssert),
                1
            };
            yield return new object[]
            {
                typeof(SqlServerRepository),
                typeof(IMessageProducer<RetryDurableLatestConsumptionSqlServerProducer>),
                typeof(RetryDurableLatestConsumptionPhysicalStorageAssert),
                1
            };
        }

        [Theory]
        [MemberData(nameof(Scenarios))]
        internal async Task RetryDurableTest(
            Type repositoryType,
            Type producerType,
            Type physicalStorageType,
            int numberOfTimesThatEachMessageIsTriedWhenDone)
        {
            // Arrange
            var numberOfMessages = 5;
            var numberOfMessagesByEachSameKey = 10;
            var numberOfTimesThatEachMessageIsTriedBeforeDurable = 4;
            var numberOfTimesThatEachMessageIsTriedDuringDurable = 2;
            var producer = this.serviceProvider.GetRequiredService(producerType) as IMessageProducer;
            var physicalStorageAssert = this.serviceProvider.GetRequiredService(physicalStorageType) as IPhysicalStorageAssert;
            var messages = this.fixture.CreateMany<RetryDurableTestMessage>(numberOfMessages).ToList();
            await this.repositoryProvider.GetRepositoryOfType(repositoryType).CleanDatabaseAsync().ConfigureAwait(false);
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
                await InMemoryAuxiliarStorage.AssertCountRetryDurableMessageAsync(message, numberOfTimesThatEachMessageIsTriedBeforeDurable).ConfigureAwait(false);
            }

            foreach (var message in messages)
            {
                await physicalStorageAssert.AssertRetryDurableMessageCreationAsync(repositoryType, message, numberOfMessagesByEachSameKey).ConfigureAwait(false);
            }

            // Assert - Retrying
            InMemoryAuxiliarStorage.Clear();

            foreach (var message in messages)
            {
                await InMemoryAuxiliarStorage.AssertCountRetryDurableMessageAsync(message, numberOfTimesThatEachMessageIsTriedDuringDurable).ConfigureAwait(false);
            }

            foreach (var message in messages)
            {
                await physicalStorageAssert.AssertRetryDurableMessageRetryingAsync(repositoryType, message, numberOfTimesThatEachMessageIsTriedDuringDurable).ConfigureAwait(false);
            }

            // Assert - Done
            InMemoryAuxiliarStorage.ThrowException = false;
            InMemoryAuxiliarStorage.Clear();

            foreach (var message in messages)
            {
                await InMemoryAuxiliarStorage.AssertCountRetryDurableMessageAsync(message, numberOfTimesThatEachMessageIsTriedWhenDone).ConfigureAwait(false);
            }

            foreach (var message in messages)
            {
                await physicalStorageAssert.AssertRetryDurableMessageDoneAsync(repositoryType, message).ConfigureAwait(false);
            }
        }
    }
}