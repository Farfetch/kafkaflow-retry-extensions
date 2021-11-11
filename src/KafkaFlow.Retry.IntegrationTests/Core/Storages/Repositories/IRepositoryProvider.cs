namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories
{
    using System;
    using System.Collections.Generic;

    internal interface IRepositoryProvider
    {
        IEnumerable<IRepository> GetAllRepositories();

        IRepository GetRepositoryOfType(RepositoryType repositoryType);

       
    }
}