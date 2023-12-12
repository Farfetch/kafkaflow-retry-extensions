using Microsoft.Extensions.Configuration;
using Xunit;

namespace KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;

[CollectionDefinition("BootstrapperRepositoryCollection")]
public class BootstrapperRepositoryCollectionFixture : ICollectionFixture<BootstrapperRepositoryFixture>
{ }

public class BootstrapperRepositoryFixture : BootstrapperFixtureTemplate
{
    public BootstrapperRepositoryFixture()
    {
            var config = new ConfigurationBuilder()
              .AddJsonFile(ConfigurationFilePath)
              .Build();

            this.InitializeDatabasesAsync(config).GetAwaiter().GetResult();
        }

    public override void Dispose()
    {
            var repositories = this.RepositoryProvider.GetAllRepositories();

            foreach (var repository in repositories)
            {
                repository.CleanDatabaseAsync().GetAwaiter().GetResult();
            }
        }
}