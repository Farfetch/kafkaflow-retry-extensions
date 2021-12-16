namespace KafkaFlow.Retry.IntegrationTests.RepositoryTests.RetryQueueDataProviderTests
{
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using KafkaFlow.Retry.Durable.Repository.Actions.Create;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;
    using Xunit;

    public class SaveToQueueTests : RetryQueueDataProviderTestsTemplate
    {
        public SaveToQueueTests(BootstrapperRepositoryFixture bootstrapperRepositoryFixture)
                   : base(bootstrapperRepositoryFixture)
        {
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