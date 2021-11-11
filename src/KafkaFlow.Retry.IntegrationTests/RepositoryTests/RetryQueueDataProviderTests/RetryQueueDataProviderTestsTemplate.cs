namespace KafkaFlow.Retry.IntegrationTests.RepositoryTests.RetryQueueDataProviderTests
{
    using KafkaFlow.Retry.Durable.Repository.Model;
    using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;
    using Xunit;

    [Collection("BootstrapperRepositoryCollection")]
    public abstract class RetryQueueDataProviderTestsTemplate
    {
        private readonly BootstrapperRepositoryFixture bootstrapperRepositoryFixture;

        protected RetryQueueDataProviderTestsTemplate(BootstrapperRepositoryFixture bootstrapperRepositoryFixture)
        {
            this.bootstrapperRepositoryFixture = bootstrapperRepositoryFixture;
        }

        protected RetryQueue GetDefaultQueue()
        {
            return new RetryQueueBuilder()
                .WithDefaultItem()
                .Build();
        }

        private protected IRepository GetRepository(RepositoryType repositoryType)
        {
            return this.bootstrapperRepositoryFixture.RepositoryProvider.GetRepositoryOfType(repositoryType);
        }
    }
}