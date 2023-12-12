using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
using KafkaFlow.Retry.IntegrationTests.Core.Messages;
using KafkaFlow.Retry.IntegrationTests.Core.Producers;
using KafkaFlow.Retry.IntegrationTests.Core.Storages;
using KafkaFlow.Retry.IntegrationTests.Core.Storages.Assertion;
using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace KafkaFlow.Retry.IntegrationTests;

[Collection("BootstrapperHostCollection")]
public class RetryDurableTests
{
    private readonly Fixture _fixture = new Fixture();
    private readonly IRepositoryProvider _repositoryProvider;
    private readonly IServiceProvider _serviceProvider;

    public RetryDurableTests(BootstrapperHostFixture bootstrapperHostFixture)
    {
        _serviceProvider = bootstrapperHostFixture.ServiceProvider;
        _repositoryProvider = bootstrapperHostFixture.ServiceProvider.GetRequiredService<IRepositoryProvider>();
        InMemoryAuxiliarStorage<RetryDurableTestMessage>.Clear();
        InMemoryAuxiliarStorage<RetryDurableTestMessage>.ThrowException = true;
    }

    public static IEnumerable<object[]> Scenarios()
    {
        yield return new object[]
        {
            RepositoryType.MongoDb,
            typeof(IMessageProducer<RetryDurableGuaranteeOrderedConsumptionMongoDbProducer>),
            typeof(RetryDurableGuaranteeOrderedConsumptionPhysicalStorageAssert),
            10
        };
        yield return new object[]
        {
            RepositoryType.SqlServer,
            typeof(IMessageProducer<RetryDurableGuaranteeOrderedConsumptionSqlServerProducer>),
            typeof(RetryDurableGuaranteeOrderedConsumptionPhysicalStorageAssert),
            10
        };
        yield return new object[]
        {
            RepositoryType.Postgres,
            typeof(IMessageProducer<RetryDurableGuaranteeOrderedConsumptionPostgresProducer>),
            typeof(RetryDurableGuaranteeOrderedConsumptionPhysicalStorageAssert),
            10
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
        yield return new object[]
        {
            RepositoryType.Postgres,
            typeof(IMessageProducer<RetryDurableLatestConsumptionPostgresProducer>),
            typeof(RetryDurableLatestConsumptionPhysicalStorageAssert),
            1
        };
    }

    [Theory]
    [MemberData(nameof(Scenarios))]
    internal async Task RetryDurableTest(
        RepositoryType repositoryType,
        Type producerType,
        Type physicalStorageType,
        int numberOfTimesThatEachMessageIsTriedWhenDone)
    {
        // Arrange
        var numberOfMessages = 5;
        var numberOfMessagesByEachSameKey = 10;
        var numberOfTimesThatEachMessageIsTriedBeforeDurable = 4;
        var numberOfTimesThatEachMessageIsTriedDuringDurable = 2;
        var producer = _serviceProvider.GetRequiredService(producerType) as IMessageProducer;
        var physicalStorageAssert = _serviceProvider.GetRequiredService(physicalStorageType) as IPhysicalStorageAssert;
        var messages = _fixture.CreateMany<RetryDurableTestMessage>(numberOfMessages).ToList();
        await _repositoryProvider.GetRepositoryOfType(repositoryType).CleanDatabaseAsync();
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
            await InMemoryAuxiliarStorage<RetryDurableTestMessage>.AssertCountMessageAsync(message, numberOfTimesThatEachMessageIsTriedBeforeDurable);
        }

        foreach (var message in messages)
        {
            await physicalStorageAssert.AssertRetryDurableMessageCreationAsync(repositoryType, message, numberOfMessagesByEachSameKey);
        }

        // Assert - Retrying
        InMemoryAuxiliarStorage<RetryDurableTestMessage>.Clear();

        foreach (var message in messages)
        {
            await InMemoryAuxiliarStorage<RetryDurableTestMessage>.AssertCountMessageAsync(message, numberOfTimesThatEachMessageIsTriedDuringDurable);
        }

        foreach (var message in messages)
        {
            await physicalStorageAssert.AssertRetryDurableMessageRetryingAsync(repositoryType, message, numberOfTimesThatEachMessageIsTriedDuringDurable);
        }

        // Assert - Done
        InMemoryAuxiliarStorage<RetryDurableTestMessage>.ThrowException = false;
        InMemoryAuxiliarStorage<RetryDurableTestMessage>.Clear();

        foreach (var message in messages)
        {
            await InMemoryAuxiliarStorage<RetryDurableTestMessage>.AssertCountMessageAsync(message, numberOfTimesThatEachMessageIsTriedWhenDone);
        }

        foreach (var message in messages)
        {
            await physicalStorageAssert.AssertRetryDurableMessageDoneAsync(repositoryType, message);
        }
    }
}