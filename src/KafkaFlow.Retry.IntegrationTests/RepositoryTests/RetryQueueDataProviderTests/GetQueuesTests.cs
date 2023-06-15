namespace KafkaFlow.Retry.IntegrationTests.RepositoryTests.RetryQueueDataProviderTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using KafkaFlow.Retry.Durable.Common;
    using KafkaFlow.Retry.Durable.Repository.Actions.Read;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;
    using Xunit;

    public class GetQueuesTests : RetryQueueDataProviderTestsTemplate
    {
        public GetQueuesTests(BootstrapperRepositoryFixture bootstrapperRepositoryFixture)
              : base(bootstrapperRepositoryFixture)
        {
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        public async Task GetQueuesAsync_DifferentSearchGroupKeyDifferentQueueStatusDifferentItemStatus_ReturnOnlyRequestedQueuesAndItems(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var searchGroupKeyA = Guid.NewGuid().ToString();

            var inputToGetSearchGroupKeyA = this.GetQueuesInput(searchGroupKeyA);

            var searchGroupKeyB = Guid.NewGuid().ToString();

            var queueA1 = new RetryQueueBuilder()
                                        .WithSearchGroupKey(searchGroupKeyA)
                                        .WithStatus(RetryQueueStatus.Done)
                                        .CreateItem().WithDoneStatus().AddItem()
                                        .Build();

            var queueA2 = new RetryQueueBuilder()
                                        .WithSearchGroupKey(searchGroupKeyA)
                                        .WithStatus(RetryQueueStatus.Active)
                                        .CreateItem().WithInRetryStatus().AddItem()
                                        .Build();

            var queueA3 = new RetryQueueBuilder()
                                        .WithSearchGroupKey(searchGroupKeyA)
                                        .WithStatus(RetryQueueStatus.Active)
                                        .CreateItem().WithWaitingStatus().AddItem()
                                        .Build();

            var queueB = new RetryQueueBuilder()
                                        .WithSearchGroupKey(searchGroupKeyB)
                                        .WithStatus(RetryQueueStatus.Active)
                                        .CreateItem().WithWaitingStatus().AddItem()
                                        .Build();

            await repository.CreateQueueAsync(queueA1);
            await repository.CreateQueueAsync(queueA2);
            await repository.CreateQueueAsync(queueA3);
            await repository.CreateQueueAsync(queueB);

            // Act
            var result = await repository.RetryQueueDataProvider.GetQueuesAsync(inputToGetSearchGroupKeyA);

            // Assert
            result.RetryQueues.Should().NotBeNullOrEmpty();

            // case A2 and case A3
            result.RetryQueues.Should().HaveCount(2);

            var actualQueueA2 = result.RetryQueues.SingleOrDefault(q => q.QueueGroupKey == queueA2.QueueGroupKey);
            actualQueueA2.Should().NotBeNull();

            var actualQueueA3 = result.RetryQueues.SingleOrDefault(q => q.QueueGroupKey == queueA3.QueueGroupKey);
            actualQueueA3.Should().NotBeNull();

            actualQueueA2.Items.Should().BeEmpty();
            actualQueueA3.Items.Should().ContainSingle();
            actualQueueA3.Items.Should().OnlyContain(i => i.Status == RetryQueueItemStatus.Waiting);
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        public async Task GetQueuesAsync_ExistingQueue_ReturnsQueue(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var queue = this.GetDefaultQueue();

            var input = this.GetQueuesInput(queue.SearchGroupKey);

            await repository.CreateQueueAsync(queue);

            // Act
            var result = await repository.RetryQueueDataProvider.GetQueuesAsync(input);

            // Assert
            result.RetryQueues.Should().NotBeNullOrEmpty();

            var actualQueue = result.RetryQueues.FirstOrDefault(x => x.QueueGroupKey == queue.QueueGroupKey);

            actualQueue.Should().NotBeNull();
            actualQueue.Status.Should().Be(input.Status);
            actualQueue.Items.Should().NotBeNullOrEmpty();
            actualQueue.Items.Should().OnlyContain(x => input.ItemsStatuses.Contains(x.Status));
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        public async Task GetQueuesAsync_ExistingQueuesActiveButDifferentSearchGroupKey_DontReturnQueues(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var differentSearchGroupKey = "differentSearchGroupKey";

            var queue1 = new RetryQueueBuilder()
                                        .WithSearchGroupKey(differentSearchGroupKey)
                                        .WithStatus(RetryQueueStatus.Done)
                                        .CreateItem().WithDoneStatus().AddItem()
                                        .Build();

            var input = this.GetQueuesInput(differentSearchGroupKey);

            var queue2 = this.GetDefaultQueue();

            await repository.CreateQueueAsync(queue1);
            await repository.CreateQueueAsync(queue2);

            // Act
            var result = await repository.RetryQueueDataProvider.GetQueuesAsync(input);

            // Assert
            result.RetryQueues.Should().BeEmpty();

            var actualQueue1 = await repository.GetAllRetryQueueDataAsync(queue1.QueueGroupKey);
            actualQueue1.Should().NotBeNull();

            var actualQueue2 = await repository.GetAllRetryQueueDataAsync(queue2.QueueGroupKey);
            actualQueue2.Should().NotBeNull();
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        public async Task GetQueuesAsync_ExistingQueueWithDifferentItemStatus_ReturnQueueWithoutItems(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var queue = new RetryQueueBuilder()
                              .CreateItem().WithDoneStatus().AddItem()
                              .CreateItem().WithInRetryStatus().AddItem()
                              .Build();

            var input = this.GetQueuesInput(queue.SearchGroupKey);

            await repository.CreateQueueAsync(queue);

            // Act
            var result = await repository.RetryQueueDataProvider.GetQueuesAsync(input);

            // Assert
            result.RetryQueues.Should().NotBeNullOrEmpty();

            var actualQueue = result.RetryQueues.FirstOrDefault(x => x.QueueGroupKey == queue.QueueGroupKey);

            actualQueue.Should().NotBeNull();
            actualQueue.Items.Should().BeEmpty();
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        public async Task GetQueuesAsync_ExistingQueueWithDifferentQueueStatus_DontReturnQueues(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var queue = new RetryQueueBuilder()
                                .WithSearchGroupKey(Guid.NewGuid().ToString())
                                .WithStatus(RetryQueueStatus.Done)
                                .CreateItem().WithDoneStatus().AddItem()
                                .Build();

            var input = this.GetQueuesInput(queue.SearchGroupKey);

            await repository.CreateQueueAsync(queue);

            // Act
            var result = await repository.RetryQueueDataProvider.GetQueuesAsync(input);

            // Assert
            result.RetryQueues.Should().BeEmpty();
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        public async Task GetQueuesAsync_ExistingQueueWithDistinctItemStatus_ReturnsQueueWithFilteredItems(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var queue = new RetryQueueBuilder()
                           .CreateItem().WithDoneStatus().AddItem()
                           .CreateItem().WithInRetryStatus().AddItem()
                           .CreateItem().WithWaitingStatus().AddItem()
                           .CreateItem().WithWaitingStatus().AddItem()
                           .Build();

            var input = this.GetQueuesInput(queue.SearchGroupKey);

            await repository.CreateQueueAsync(queue);

            // Act
            var result = await repository.RetryQueueDataProvider.GetQueuesAsync(input);

            // Assert
            result.RetryQueues.Should().NotBeNullOrEmpty();

            var actualQueue = result.RetryQueues.FirstOrDefault(x => x.QueueGroupKey == queue.QueueGroupKey);

            actualQueue.Should().NotBeNull();
            actualQueue.Status.Should().Be(input.Status);
            actualQueue.Items.Should().NotBeNullOrEmpty();
            actualQueue.Items.Should().OnlyContain(x => input.ItemsStatuses.Contains(x.Status));
            actualQueue.Items.Should().HaveCount(queue.Items.Count(i => input.ItemsStatuses.Contains(i.Status)));
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        public async Task GetQueuesAsync_ItemsWithDifferentModifiedDates_ReturnQueueWithItemsandQueueWithNoItem(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var searchGroupKeyA = Guid.NewGuid().ToString();

            var inputToGetSearchGroupKeyA = this.GetQueuesWithStuckStatusInput(searchGroupKeyA);

            var queueA1 = new RetryQueueBuilder()
                                        .WithSearchGroupKey(searchGroupKeyA)
                                        .WithStatus(RetryQueueStatus.Active)
                                        .CreateItem()
                                            .WithInRetryStatus()
                                            .WithModifiedStatusDate(new DateTime(1992, 3, 3, 3, 3, 3))
                                            .AddItem()
                                        .Build();

            var queueA2 = new RetryQueueBuilder()
                                        .WithSearchGroupKey(searchGroupKeyA)
                                        .WithStatus(RetryQueueStatus.Active)
                                        .CreateItem()
                                            .WithInRetryStatus()
                                            .WithModifiedStatusDate(null)
                                            .AddItem()
                                        .Build();

            var queueA3 = new RetryQueueBuilder()
                                        .WithSearchGroupKey(searchGroupKeyA)
                                        .WithStatus(RetryQueueStatus.Active)
                                        .CreateItem()
                                            .WithWaitingStatus()
                                            .WithModifiedStatusDate(null)
                                            .AddItem()
                                        .Build();

            await repository.CreateQueueAsync(queueA1);
            await repository.CreateQueueAsync(queueA2);
            await repository.CreateQueueAsync(queueA3);

            // Act
            var result = await repository.RetryQueueDataProvider.GetQueuesAsync(inputToGetSearchGroupKeyA);

            // Assert
            result.RetryQueues.Should().NotBeNullOrEmpty();

            result.RetryQueues.Should().HaveCount(3);

            var actualQueueA1 = result.RetryQueues.SingleOrDefault(q => q.QueueGroupKey == queueA1.QueueGroupKey);
            var actualQueueA2 = result.RetryQueues.SingleOrDefault(q => q.QueueGroupKey == queueA2.QueueGroupKey);
            var actualQueueA3 = result.RetryQueues.SingleOrDefault(q => q.QueueGroupKey == queueA3.QueueGroupKey);

            actualQueueA1.Items.Should().ContainSingle();
            actualQueueA2.Items.Should().BeEmpty();
            actualQueueA3.Items.Should().ContainSingle();
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        public async Task GetQueuesAsync_WithSeverityLevel_ReturnsOnlyItemsWithCorrespondingLevel(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var input = this.GetQueuesInputWithSeverites(new SeverityLevel[] { SeverityLevel.High, SeverityLevel.Medium });

            var queueA = new RetryQueueBuilder()
                                .WithSearchGroupKey("searchGroupKeyA")
                                .WithStatus(RetryQueueStatus.Active)
                                .CreateItem().WithWaitingStatus().WithSeverityLevel(SeverityLevel.Low).AddItem()
                                .CreateItem().WithWaitingStatus().WithSeverityLevel(SeverityLevel.Medium).AddItem()
                                .CreateItem().WithWaitingStatus().WithSeverityLevel(SeverityLevel.High).AddItem()
                                .Build();

            var queueB = new RetryQueueBuilder()
                                .WithSearchGroupKey("searchGroupKeyB")
                                .WithStatus(RetryQueueStatus.Active)
                                .CreateItem().WithDoneStatus().AddItem()
                                .CreateItem().WithWaitingStatus().WithSeverityLevel(SeverityLevel.Low).AddItem()
                                .Build();

            await repository.CreateQueueAsync(queueA);
            await repository.CreateQueueAsync(queueB);

            // Act
            var result = await repository.RetryQueueDataProvider.GetQueuesAsync(input);

            // Assert
            var actualQueues = result.RetryQueues.Where(
                r => r.SearchGroupKey == queueA.SearchGroupKey ||
                r.SearchGroupKey == queueB.SearchGroupKey);

            actualQueues.Count().Should().Be(2);

            actualQueues.SelectMany(o => o.Items.Where(i => i.SeverityLevel == SeverityLevel.Low)).Count().Should().Be(0);
            actualQueues.SelectMany(o => o.Items.Where(i => i.SeverityLevel == SeverityLevel.Medium)).Count().Should().Be(1);
            actualQueues.SelectMany(o => o.Items.Where(i => i.SeverityLevel == SeverityLevel.High)).Count().Should().Be(1);
        }

        [Theory]
        [InlineData(RepositoryType.MongoDb)]
        [InlineData(RepositoryType.SqlServer)]
        public async Task GetQueuesAsync_WithStuckStatusFilter_ReturnsItemsByStatusAndStuckItems(RepositoryType repositoryType)
        {
            // Arrange
            var repository = this.GetRepository(repositoryType);

            var expirationInterval = new TimeSpan(1, 0, 0); // 1 hour

            var expectedTotalStuckedItems = 1;
            var expectedStuckedItemSort = 1;

            var queue = new RetryQueueBuilder()
                                        .CreateItem()
                                            .WithDoneStatus()
                                            .AddItem()
                                        .CreateItem()
                                            .WithInRetryStatus()
                                            .WithModifiedStatusDate(DateTime.UtcNow.AddDays(-1))
                                            .AddItem()
                                        .CreateItem()
                                            .WithInRetryStatus()
                                            .WithModifiedStatusDate(DateTime.UtcNow.AddMinutes(-10))
                                            .AddItem()
                                        .CreateItem()
                                            .WithWaitingStatus()
                                            .AddItem()
                                        .Build();

            var input = this.GetQueuesInputWithStuckStatusFilter(expirationInterval, queue.SearchGroupKey);

            await repository.CreateQueueAsync(queue);

            // Act
            var result = await repository.RetryQueueDataProvider.GetQueuesAsync(input);

            // Assert
            result.RetryQueues.Should().NotBeNullOrEmpty();

            var actualQueue = result.RetryQueues.FirstOrDefault(x => x.QueueGroupKey == queue.QueueGroupKey);

            actualQueue.Should().NotBeNull();
            actualQueue.Status.Should().Be(input.Status);
            actualQueue.Items.Should().NotBeNullOrEmpty();

            var itemsObtainedByStatus = actualQueue.Items.Where(i => input.ItemsStatuses.Contains(i.Status));
            var stuckedItems = actualQueue.Items.Where(i => i.Status == input.StuckStatusFilter.ItemStatus);

            stuckedItems.Should().HaveCount(expectedTotalStuckedItems);
            stuckedItems.First().Sort.Should().Be(expectedStuckedItemSort);

            actualQueue.Items.Should().HaveCount(itemsObtainedByStatus.Count() + stuckedItems.Count());
        }

        private GetQueuesInput GetQueuesInput(string searchGroupKey)
        {
            return new GetQueuesInput(
                RetryQueueStatus.Active,
                new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting },
                GetQueuesSortOption.ByLastExecution_Ascending,
                100)
            {
                SearchGroupKey = searchGroupKey
            };
        }

        private GetQueuesInput GetQueuesInputWithSeverites(IEnumerable<SeverityLevel> severities)
        {
            return new GetQueuesInput(
               RetryQueueStatus.Active,
               new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting },
               GetQueuesSortOption.ByLastExecution_Ascending,
               100)
            {
                SeverityLevels = severities
            };
        }

        private GetQueuesInput GetQueuesInputWithStuckStatusFilter(TimeSpan expirationInterval, string searchGroupKey)
        {
            return new GetQueuesInput(
                RetryQueueStatus.Active,
                new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting },
                GetQueuesSortOption.ByLastExecution_Ascending,
                100,
                new StuckStatusFilter(RetryQueueItemStatus.InRetry, expirationInterval))
            {
                SearchGroupKey = searchGroupKey
            };
        }

        private GetQueuesInput GetQueuesWithStuckStatusInput(string searchGroupKey)
        {
            return new GetQueuesInput(
                RetryQueueStatus.Active,
                new RetryQueueItemStatus[] { RetryQueueItemStatus.Waiting },
                GetQueuesSortOption.ByLastExecution_Ascending,
                100,
                stuckStatusFilter: new StuckStatusFilter(RetryQueueItemStatus.InRetry, new TimeSpan(0, 3, 0)))
            {
                SearchGroupKey = searchGroupKey
            };
        }
    }
}