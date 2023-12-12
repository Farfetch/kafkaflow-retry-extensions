using System.Collections.Generic;
using System.Linq;

namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

internal class RepositoryProvider : IRepositoryProvider
{
    private readonly IEnumerable<IRepository> _repositories;

    public RepositoryProvider(IEnumerable<IRepository> repositories)
    {
        _repositories = repositories;
    }

    public IEnumerable<IRepository> GetAllRepositories() => _repositories;

    public IRepository GetRepositoryOfType(RepositoryType repositoryType)
    {
        return _repositories.Single(r => r.RepositoryType == repositoryType);
    }
}