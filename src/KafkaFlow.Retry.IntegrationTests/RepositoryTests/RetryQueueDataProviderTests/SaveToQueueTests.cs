namespace KafkaFlow.Retry.IntegrationTests.RepositoryTests.RetryQueueDataProviderTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;
    using Xunit;

    public class SaveToQueueTests : RetryQueueDataProviderTestsTemplate
    {
        public SaveToQueueTests(BootstrapperHostFixture bootstrapperRepositoryFixture)
                   : base(bootstrapperRepositoryFixture)
        {
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        public async Task SaveToQueueAsync_ExistingQueueWithOneItem_ReturnsAddedStatus(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var saveToQueueInput = new RetryQueueBuilder()
                 .CreateItem()
                    .WithWaitingStatus()
                    .WithSeverityLevel(SeverityLevel.High)
                    .AddItem()
                 .BuildAsInput();

            var queue = new RetryQueueBuilder()
                             .WithQueueGroupKey(saveToQueueInput.QueueGroupKey)
                             .CreateItem()
                               .WithInRetryStatus()
                               .WithSeverityLevel(SeverityLevel.Medium)
                               .AddItem()
                             .Build();

            await repository.CreateQueueAsync(queue);

            // Act
            var result = await repository.RetryQueueDataProvider.SaveToQueueAsync(saveToQueueInput);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(SaveToQueueResultStatus.Added);

            var actualQueue = await repository.GetAllRetryQueueDataAsync(saveToQueueInput.QueueGroupKey);

            actualQueue.Should().NotBeNull();
            actualQueue.Status.Should().Be(RetryQueueStatus.Active);
            actualQueue.Items.Should().NotBeNullOrEmpty().And.HaveCount(2).And.Contain(x => x.Status == RetryQueueItemStatus.Waiting);
            actualQueue.Items.Should().ContainSingle(x => x.SeverityLevel == SeverityLevel.Medium);
            actualQueue.Items.Should().ContainSingle(x => x.SeverityLevel == SeverityLevel.High);
            actualQueue.Items.ElementAt(1).Description.Should().Be(saveToQueueInput.Description);
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        public async Task SaveToQueueAsync_ExistingQueueWithStatusDone_ReturnsAddedStatus(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var saveToQueueInput = new RetryQueueBuilder()
                 .WithDefaultItem()
                 .BuildAsInput();

            var queue = new RetryQueueBuilder()
                              .WithQueueGroupKey(saveToQueueInput.QueueGroupKey)
                              .WithStatus(RetryQueueStatus.Done)
                                .CreateItem()
                                    .WithDoneStatus()
                                    .AddItem()
                              .Build();

            await repository.CreateQueueAsync(queue);

            // Act
            var result = await repository.RetryQueueDataProvider.SaveToQueueAsync(saveToQueueInput);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(SaveToQueueResultStatus.Added);

            var actualQueue = await repository.GetAllRetryQueueDataAsync(saveToQueueInput.QueueGroupKey);

            actualQueue.Should().NotBeNull();
            actualQueue.Status.Should().Be(RetryQueueStatus.Active);
            actualQueue.Items.Should().NotBeNullOrEmpty()
                .And.HaveCount(2)
                .And.Contain(x => x.Status == RetryQueueItemStatus.Waiting)
                .And.Contain(x => x.Status == RetryQueueItemStatus.Done);
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        public async Task SaveToQueueAsync_NonExistingQueue_ReturnsCreatedStatus(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var saveToQueueInput = new RetryQueueBuilder()
                 .WithDefaultItem()
                 .BuildAsInput();

            // Act
            var result = await repository.RetryQueueDataProvider.SaveToQueueAsync(saveToQueueInput);

            // Assert
            result.Should().NotBeNull();
            result.Status.Should().Be(SaveToQueueResultStatus.Created);

            var queue = await repository.GetAllRetryQueueDataAsync(saveToQueueInput.QueueGroupKey);

            queue.Should().NotBeNull();
            queue.Status.Should().Be(RetryQueueStatus.Active);

            queue.Items.Should().NotBeNullOrEmpty().And.HaveCount(1).And.OnlyContain(x => x.Status == RetryQueueItemStatus.Waiting);

            queue.Items.First().Message.Should().NotBeNull();
            queue.Items.First().Message.Headers.Should().NotBeNull().And.HaveSameCount(saveToQueueInput.Message.Headers);
        }
    }
}