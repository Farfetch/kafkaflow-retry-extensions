namespace KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories
{
    using System.Collections.Generic;
    using System.Linq;

    internal class RepositoryProvider : IRepositoryProvider
    {
        private readonly IEnumerable<IRepository> repositories;

        public RepositoryProvider(IEnumerable<IRepository> repositories)
        {
            this.repositories = repositories;
        }

        public IEnumerable<IRepository> GetAllRepositories() => this.repositories;

        public IRepository GetRepositoryOfType(RepositoryType repositoryType)
        {
            return this.repositories.First(r => r.RepositoryType == RepositoryType.MongoDb);

            //return this.repositories.Single(r => r.RepositoryType == repositoryType);
        }
    }
}