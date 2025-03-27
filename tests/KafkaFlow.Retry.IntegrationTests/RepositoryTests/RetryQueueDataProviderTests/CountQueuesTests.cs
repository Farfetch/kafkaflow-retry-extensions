using System;
using System.Threading.Tasks;
using FluentAssertions;
using KafkaFlow.Retry.Durable.Repository.Actions.Read;
using KafkaFlow.Retry.Durable.Repository.Model;
using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
using KafkaFlow.Retry.IntegrationTests.Core.Storages;
using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

namespace KafkaFlow.Retry.IntegrationTests.RepositoryTests.RetryQueueDataProviderTests;

public class CountQueuesTests : RetryQueueDataProviderTestsTemplate
{
    public CountQueuesTests(BootstrapperRepositoryFixture bootstrapperRepositoryFixture)
        : base(bootstrapperRepositoryFixture)
    { }

    [Theory]
    [InlineData(RepositoryType.MongoDb)]
    [InlineData(RepositoryType.SqlServer)]
    [InlineData(RepositoryType.Postgres)]
    public async Task f(RepositoryType repositoryType)
    {
        // Arrange
        var repository = GetRepository(repositoryType);

        var searchGroupKey = Guid.NewGuid().ToString();

        var queueActive = new RetryQueueBuilder()
            .WithSearchGroupKey(searchGroupKey)
            .WithStatus(RetryQueueStatus.Active)
            .CreateItem().WithWaitingStatus().AddItem()
            .Build();

        var queueDone = new RetryQueueBuilder()
            .WithSearchGroupKey(searchGroupKey)
            .WithStatus(RetryQueueStatus.Done)
            .CreateItem().WithWaitingStatus().AddItem()
            .Build();

        await repository.CreateQueueAsync(queueActive);
        await repository.CreateQueueAsync(queueDone);

        // Act
        var resultActive = await repository.RetryQueueDataProvider.CountQueuesAsync(new CountQueuesInput(RetryQueueStatus.Active) { SearchGroupKey = searchGroupKey });
        var resultDone = await repository.RetryQueueDataProvider.CountQueuesAsync(new CountQueuesInput(RetryQueueStatus.Done) { SearchGroupKey = searchGroupKey });

        // Assert
        resultActive.Should().Be(1);
        resultDone.Should().Be(1);
    }
}