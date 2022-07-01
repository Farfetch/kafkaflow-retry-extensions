namespace KafkaFlow.Retry.IntegrationTests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;
    using Confluent.Kafka;
    using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers;
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
        private const int defaultWaitingTimeSeconds = 240;
        private readonly BootstrapperHostFixture bootstrapperHostFixture;
        private readonly IRepositoryProvider repositoryProvider;
        private readonly IServiceProvider serviceProvider;
        private RepositoryType testRepositoryType;

        public EmptyPartitionKeyRetryDurableTests(BootstrapperHostFixture bootstrapperHostFixture)
        {
            this.serviceProvider = bootstrapperHostFixture.ServiceProvider;
            this.repositoryProvider = bootstrapperHostFixture.ServiceProvider.GetRequiredService<IRepositoryProvider>();
            InMemoryAuxiliarStorage<RetryDurableTestMessage>.Clear();
            InMemoryAuxiliarStorage<RetryDurableTestMessage>.ThrowException = true;
            this.bootstrapperHostFixture = bootstrapperHostFixture;
        }

        ~EmptyPartitionKeyRetryDurableTests()
        {
            BootstrapperKafka.RecreateKafkaTopicsAsync(bootstrapperHostFixture.KafkaSettings.Brokers, new string[] {
                "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-mongo-db",
                "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-mongo-db-retry",
                "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-sql-server",
                "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-sql-server-retry",
                "test-kafka-flow-retry-retry-durable-latest-consumption-mongo-db",
                "test-kafka-flow-retry-retry-durable-latest-consumption-mongo-db-retry",
                "test-kafka-flow-retry-retry-durable-latest-consumption-sql-server",
                "test-kafka-flow-retry-retry-durable-latest-consumption-sql-server-retry"
            })
            .GetAwaiter()
            .GetResult();

            repositoryProvider.GetRepositoryOfType(testRepositoryType).CleanDatabaseAsync().GetAwaiter().GetResult();
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

        public static IEnumerable<object[]> NullKeyScenarios()
        {
            yield return new object[]
            {
                RepositoryType.MongoDb,
                typeof(RetryDurableGuaranteeOrderedConsumptionPhysicalStorageAssert),
                "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-mongo-db",
                2 //numberOfMessagesToBeProduced
            };
            yield return new object[]
            {
                RepositoryType.SqlServer,
                typeof(RetryDurableGuaranteeOrderedConsumptionPhysicalStorageAssert),
                "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-sql-server",
                2
            };
            yield return new object[]
            {
                RepositoryType.MongoDb,
                typeof(RetryDurableLatestConsumptionPhysicalStorageAssert),
                "test-kafka-flow-retry-retry-durable-latest-consumption-mongo-db",
                1
            };
            yield return new object[]
            {
                RepositoryType.SqlServer,
                typeof(RetryDurableLatestConsumptionPhysicalStorageAssert),
                "test-kafka-flow-retry-retry-durable-latest-consumption-sql-server",
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
            testRepositoryType = repositoryType;
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

            await AssertRetryAndConsumeMessages(repositoryType,
                numberOfMessagesByEachSameKey,
                numberOfTimesThatEachMessageIsTriedWhenDone,
                numberOfTimesThatEachMessageIsTriedDuringDurable,
                physicalStorageAssert,
                messageToValidate
            ).ConfigureAwait(false);
        }

        [Theory]
        [MemberData(nameof(NullKeyScenarios))]
        internal async Task NullKeyRetryDurableTest(
            RepositoryType repositoryType,
            Type physicalStorageType,
            string topicName,
            int numberOfMessagesToBeProduced)
        {
            // Arrange
            testRepositoryType = repositoryType;
            var numberOfMessagesByEachSameKey = 1;
            var numberOfTimesThatEachMessageIsTriedWhenDone = 1;
            var numberOfTimesThatEachMessageIsTriedDuringDurable = 1;

            var config = new ProducerConfig
            {
                BootstrapServers = bootstrapperHostFixture.KafkaSettings.Brokers,
            };

            Error actualError = null;
            var producer = new ProducerBuilder<string, RetryDurableTestMessage>(config)
                .SetValueSerializer(new RetryDurableTestMessageSerializer())
                .SetErrorHandler((producer, error) =>
                {
                    actualError = error;
                })
                .Build();

            var physicalStorageAssert = this.serviceProvider.GetRequiredService(physicalStorageType) as IPhysicalStorageAssert;

            var messages = new List<Message<string, RetryDurableTestMessage>>();
            for (int i = 0; i < numberOfMessagesToBeProduced; i++)
            {
                messages.Add(new Message<string, RetryDurableTestMessage> { Key = null, Value = new RetryDurableTestMessage { Key = null, Value = $"Message_{i + 1}" } });
            }

            // Act
            foreach (var message in messages)
            {
                await producer.ProduceAsync(topicName, message).ConfigureAwait(false);
            }

            RetryDurableTestMessage messageToValidate = messages[0].Value;

            await AssertRetryAndConsumeMessages(repositoryType,
                numberOfMessagesByEachSameKey,
                numberOfTimesThatEachMessageIsTriedWhenDone,
                numberOfTimesThatEachMessageIsTriedDuringDurable,
                physicalStorageAssert,
                messageToValidate
            ).ConfigureAwait(false);

            Assert.Null(actualError);
        }

        private static async Task AssertRetryAndConsumeMessages(RepositoryType repositoryType, int numberOfMessagesByEachSameKey, int numberOfTimesThatEachMessageIsTriedWhenDone, int numberOfTimesThatEachMessageIsTriedDuringDurable, IPhysicalStorageAssert physicalStorageAssert, RetryDurableTestMessage messageToValidate)
        {
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

        private class RetryDurableTestMessageSerializer : Confluent.Kafka.ISerializer<RetryDurableTestMessage>
        {
            public byte[] Serialize(RetryDurableTestMessage data, SerializationContext context)
            {
                if (data == null)
                {
                    return null;
                }

                return Encoding.UTF8.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(data));
            }
        }
    }
}