using System.Collections.Generic;

namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

internal interface IRepositoryProvider
{
    IEnumerable<IRepository> GetAllRepositories();

    IRepository GetRepositoryOfType(RepositoryType repositoryType);

       
}