namespace KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures
{
    using global::Microsoft.Extensions.Configuration;
    using Xunit;

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

            //this.InitializeDatabasesAsync(config).GetAwaiter().GetResult();

            this.InitializeMongoDb(config);
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
}