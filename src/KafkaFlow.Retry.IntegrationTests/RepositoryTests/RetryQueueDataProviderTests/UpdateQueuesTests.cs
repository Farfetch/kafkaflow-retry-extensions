namespace KafkaFlow.Retry.IntegrationTests.RepositoryTests.RetryQueueDataProviderTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Durable.Repository.Actions.Update;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;
    using Xunit;

    public class UpdateQueuesTests : RetryQueueDataProviderTestsTemplate
    {
        public UpdateQueuesTests(BootstrapperRepositoryFixture bootstrapperRepositoryFixture)
                 : base(bootstrapperRepositoryFixture)
        {
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        [InlineData(RepositoryType.Postgres)]
        public async Task UpdateQueuesAsync_WithInactiveQueue_ReturnsQueueIsNotActive(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var expectedItemStatus = RetryQueueItemStatus.Waiting;

            var queue = new RetryQueueBuilder()
                .WithStatus(RetryQueueStatus.Done)
                .WithDefaultItem()
                .Build();

            await repository.CreateQueueAsync(queue);

            var inputUpdate = new UpdateQueuesInput(new[] { queue.QueueGroupKey }, RetryQueueItemStatus.Cancelled);

            // Act
            var result = await repository.RetryQueueDataProvider.UpdateQueuesAsync(inputUpdate);

            // Assert
            result.Results.First().Status.Should().Be(UpdateQueueResultStatus.QueueIsNotActive);

            var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
            actualQueue.Should().NotBeNull();
            actualQueue.Items.Should().HaveCount(1);
            actualQueue.Items.Single().Status.Should().Be(expectedItemStatus);
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        [InlineData(RepositoryType.Postgres)]
        public async Task UpdateQueuesAsync_WithItems_ReturnsUpdatedQueue(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);
            var expectedItemStatus = RetryQueueItemStatus.Cancelled;

            var queue = new RetryQueueBuilder()
                .WithDefaultItem()
                .WithDefaultItem()
                .Build();

            await repository.CreateQueueAsync(queue);

            var inputUpdate = new UpdateQueuesInput(new[] { queue.QueueGroupKey }, RetryQueueItemStatus.Cancelled);

            // Act
            var result = await repository.RetryQueueDataProvider.UpdateQueuesAsync(inputUpdate);

            // Assert
            result.Results.First().Status.Should().Be(UpdateQueueResultStatus.Updated);

            var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
            actualQueue.Should().NotBeNull();
            actualQueue.Items.Should().HaveCount(2);
            actualQueue.Items.ElementAt(0).Status.Should().Be(expectedItemStatus);
            actualQueue.Items.ElementAt(1).Status.Should().Be(expectedItemStatus);
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        [InlineData(RepositoryType.Postgres)]
        public async Task UpdateQueuesAsync_WithNoItems_ReturnsQueueHasNoActiveItems(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);
            var expectedItemStatus = RetryQueueItemStatus.Done;

            var queue = new RetryQueueBuilder()
                .CreateItem().WithStatus(expectedItemStatus).AddItem()
                .Build();

            await repository.CreateQueueAsync(queue);

            var inputUpdate = new UpdateQueuesInput(new[] { queue.QueueGroupKey }, RetryQueueItemStatus.Cancelled);

            // Act
            var result = await repository.RetryQueueDataProvider.UpdateQueuesAsync(inputUpdate);

            // Assert
            result.Results.First().Status.Should().Be(UpdateQueueResultStatus.QueueHasNoItemsWaiting);

            var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
            actualQueue.Should().NotBeNull();
            actualQueue.Items.Should().HaveCount(1);
            actualQueue.Items.Single().Status.Should().Be(expectedItemStatus);
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        [InlineData(RepositoryType.Postgres)]
        public async Task UpdateQueuesAsync_WithStatusNotCancelled_ReturnsUpdatedStatusNotAllowed(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);
            var expectedItemStatus = RetryQueueItemStatus.Waiting;

            var queue = this.GetDefaultQueue();

            await repository.CreateQueueAsync(queue);

            var inputUpdate = new UpdateQueuesInput(new[] { queue.QueueGroupKey }, RetryQueueItemStatus.Done);

            // Act
            var result = await repository.RetryQueueDataProvider.UpdateQueuesAsync(inputUpdate);

            // Assert
            result.Results.First().Status.Should().Be(UpdateQueueResultStatus.UpdateIsNotAllowed);

            var actualQueue = await repository.GetAllRetryQueueDataAsync(queue.QueueGroupKey);
            actualQueue.Should().NotBeNull();
            actualQueue.Items.Should().HaveCount(1);
            actualQueue.Items.Single().Status.Should().Be(expectedItemStatus);
        }
    }
}