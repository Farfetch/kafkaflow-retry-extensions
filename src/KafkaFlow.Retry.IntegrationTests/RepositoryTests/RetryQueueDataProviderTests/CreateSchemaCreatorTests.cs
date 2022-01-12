namespace KafkaFlow.Retry.IntegrationTests.RepositoryTests.RetryQueueDataProviderTests
{
    using System.Threading.Tasks;
    using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
    using KafkaFlow.Retry.SqlServer;
    using Xunit;

    public class CreateSchemaCreatorTests : RetryQueueDataProviderTestsTemplate
    {
        public CreateSchemaCreatorTests(BootstrapperRepositoryFixture bootstrapperRepositoryFixture)
                : base(bootstrapperRepositoryFixture)
        {
        }

        [Fact]
        public async Task SqlServerDbDataProviderFactory_CreateSchemaCreator_ExecuteSuccessfully()
        {
            var sqlDataProviderFactory = new SqlServerDbDataProviderFactory();

            var connectionString = this.bootstrapperRepositoryFixture.SqlServerSettings.ConnectionString;
            var databaseName = this.bootstrapperRepositoryFixture.SqlServerSettings.DatabaseName;

            var sqlSettings = new SqlServerDbSettings(connectionString, databaseName);

            var retrySchemaCreator = sqlDataProviderFactory.CreateSchemaCreator(sqlSettings);

            await retrySchemaCreator.CreateOrUpdateSchemaAsync(databaseName);
        }
    }
}