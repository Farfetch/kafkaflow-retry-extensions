namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories
{
    using System;

    public interface IRepositoryProvider
    {
        IRepository GetRepositoryOfType(Type repositoryType);
    }
}