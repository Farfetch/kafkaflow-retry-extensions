namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories
{
    using System;

    internal interface IRepositoryProvider
    {
        IRepository GetRepositoryOfType(Type repositoryType);
    }
}