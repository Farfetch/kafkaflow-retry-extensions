namespace KafkaFlow.Retry.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;
    using KafkaFlow.Retry.IntegrationTests.Core.Producers;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Assertion;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;

    [Collection("BootstrapperHostCollection")]
    public class EmptyPartitionKeyRetryDurableTests
    {
        private const int defaultWaitingTimeSeconds = 120;
        private readonly IRepositoryProvider repositoryProvider;
        private readonly IServiceProvider serviceProvider;

        public EmptyPartitionKeyRetryDurableTests(BootstrapperHostFixture bootstrapperHostFixture)
        {
            this.serviceProvider = bootstrapperHostFixture.ServiceProvider;
            this.repositoryProvider = bootstrapperHostFixture.ServiceProvider.GetRequiredService<IRepositoryProvider>();
            InMemoryAuxiliarStorage<RetryDurableTestMessage>.Clear();
            InMemoryAuxiliarStorage<RetryDurableTestMessage>.ThrowException = true;
        }

        public static IEnumerable<object[]> EmptyKeyScenarios()
        {
            yield return new object[]
            {
                RepositoryType.MongoDb,
                typeof(IMessageProducer<RetryDurableGuaranteeOrderedConsumptionMongoDbProducer>),
                typeof(RetryDurableGuaranteeOrderedConsumptionPhysicalStorageAssert),
                3 //numberOfMessagesToBeProduced
            };
            yield return new object[]
            {
                RepositoryType.SqlServer,
                typeof(IMessageProducer<RetryDurableGuaranteeOrderedConsumptionSqlServerProducer>),
                typeof(RetryDurableGuaranteeOrderedConsumptionPhysicalStorageAssert),
                3
            };
            yield return new object[]
            {
                RepositoryType.MongoDb,
                typeof(IMessageProducer<RetryDurableLatestConsumptionMongoDbProducer>),
                typeof(RetryDurableLatestConsumptionPhysicalStorageAssert),
                1
            };
            yield return new object[]
            {
                RepositoryType.SqlServer,
                typeof(IMessageProducer<RetryDurableLatestConsumptionSqlServerProducer>),
                typeof(RetryDurableLatestConsumptionPhysicalStorageAssert),
                1
            };
        }

        [Theory]
        [MemberData(nameof(EmptyKeyScenarios))]
        internal async Task EmptyKeyRetryDurableTest(
            RepositoryType repositoryType,
            Type producerType,
            Type physicalStorageType,
            int numberOfMessagesToBeProduced)
        {
            // Arrange
            var numberOfMessagesByEachSameKey = 1;
            var numberOfTimesThatEachMessageIsTriedWhenDone = 1;
            var numberOfTimesThatEachMessageIsTriedDuringDurable = 1;
            var producer = this.serviceProvider.GetRequiredService(producerType) as IMessageProducer;
            var physicalStorageAssert = this.serviceProvider.GetRequiredService(physicalStorageType) as IPhysicalStorageAssert;
            var messages = new List<RetryDurableTestMessage>();
            for (int i = 0; i < numberOfMessagesToBeProduced; i++)
            {
                messages.Add(new RetryDurableTestMessage { Key = string.Empty, Value = $"Message_{i + 1}" });
            }

            await this.repositoryProvider.GetRepositoryOfType(repositoryType).CleanDatabaseAsync().ConfigureAwait(false);

            // Act
            foreach (var message in messages)
            {
                await producer.ProduceAsync(message.Key, message).ConfigureAwait(false);
            }

            RetryDurableTestMessage messageToValidate = messages[0];

            await physicalStorageAssert
                .AssertEmptyKeyRetryDurableMessageRetryingAsync(repositoryType, messageToValidate, numberOfMessagesByEachSameKey)
                .ConfigureAwait(false);

            // Assert - Retrying
            InMemoryAuxiliarStorage<RetryDurableTestMessage>.Clear();

            await InMemoryAuxiliarStorage<RetryDurableTestMessage>
                .AssertEmptyPartitionKeyCountMessageAsync(messageToValidate, numberOfTimesThatEachMessageIsTriedDuringDurable, defaultWaitingTimeSeconds)
                .ConfigureAwait(false);

            await physicalStorageAssert
                .AssertEmptyKeyRetryDurableMessageRetryingAsync(repositoryType, messageToValidate, numberOfTimesThatEachMessageIsTriedDuringDurable)
                .ConfigureAwait(false);

            // Assert - Done
            InMemoryAuxiliarStorage<RetryDurableTestMessage>.ThrowException = false;
            InMemoryAuxiliarStorage<RetryDurableTestMessage>.Clear();

            await InMemoryAuxiliarStorage<RetryDurableTestMessage>
                .AssertEmptyPartitionKeyCountMessageAsync(messageToValidate, numberOfTimesThatEachMessageIsTriedWhenDone, defaultWaitingTimeSeconds)
                .ConfigureAwait(false);

            await physicalStorageAssert
                .AssertRetryDurableMessageDoneAsync(repositoryType, messageToValidate)
                .ConfigureAwait(false);
        }
    }
}