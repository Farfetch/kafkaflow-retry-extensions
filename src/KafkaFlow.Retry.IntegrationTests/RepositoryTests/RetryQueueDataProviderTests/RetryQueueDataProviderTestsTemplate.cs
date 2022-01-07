namespace KafkaFlow.Retry.IntegrationTests.RepositoryTests.RetryQueueDataProviderTests
{
    using System.Linq;
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;
    using Xunit;

    [Collection("BootstrapperRepositoryCollection")]
    public abstract class RetryQueueDataProviderTestsTemplate
    {
        protected readonly BootstrapperHostFixture bootstrapperRepositoryFixture;

        protected RetryQueueDataProviderTestsTemplate(BootstrapperHostFixture bootstrapperRepositoryFixture)
        {
            this.bootstrapperRepositoryFixture = bootstrapperRepositoryFixture;
        }

        protected RetryQueue GetDefaultQueue()
        {
            return new RetryQueueBuilder()
                .WithDefaultItem()
                .Build();
        }

        protected RetryQueueItem GetQueueFirstItem(RetryQueue queue)
        {
            var minSort = queue.Items.Min(i => i.Sort);
            return queue.Items.Single(i => i.Sort == minSort);
        }

        protected RetryQueueItem GetQueueLastItem(RetryQueue queue)
        {
            var maxSort = queue.Items.Max(i => i.Sort);
            return queue.Items.Single(i => i.Sort == maxSort);
        }

        protected IRepository GetRepository(RepositoryType repositoryType)
        {
            return this.bootstrapperRepositoryFixture.RepositoryProvider.GetRepositoryOfType(repositoryType);
        }
    }
}