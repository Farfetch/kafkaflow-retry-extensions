using System.Threading.Tasks;
using KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;
using KafkaFlow.Retry.Postgres;
using KafkaFlow.Retry.SqlServer;
using Xunit;

namespace KafkaFlow.Retry.IntegrationTests.RepositoryTests.RetryQueueDataProviderTests;

public class CreateSchemaCreatorTests : RetryQueueDataProviderTestsTemplate
{
    public CreateSchemaCreatorTests(BootstrapperRepositoryFixture bootstrapperRepositoryFixture)
        : base(bootstrapperRepositoryFixture)
    {
        }

    [Fact]
    public async Task PostgresDbDataProviderFactory_CreateSchemaCreator_ExecuteSuccessfully()
    {
            var postgresDataProviderFactory = new PostgresDbDataProviderFactory();

            var connectionString = this.bootstrapperRepositoryFixture.PostgresSettings.ConnectionString;
            var databaseName = this.bootstrapperRepositoryFixture.PostgresSettings.DatabaseName;

            var postgresSettings = new PostgresDbSettings(connectionString, databaseName);

            var retrySchemaCreator = postgresDataProviderFactory.CreateSchemaCreator(postgresSettings);

            await retrySchemaCreator.CreateOrUpdateSchemaAsync(databaseName);
        }

    [Fact]
    public async Task SqlServerDbDataProviderFactory_CreateSchemaCreator_ExecuteSuccessfully()
    {
            var sqlDataProviderFactory = new SqlServerDbDataProviderFactory();

            var connectionString = this.bootstrapperRepositoryFixture.SqlServerSettings.ConnectionString;
            var databaseName = this.bootstrapperRepositoryFixture.SqlServerSettings.DatabaseName;
            var schema = this.bootstrapperRepositoryFixture.SqlServerSettings.Schema;

            var sqlSettings = new SqlServerDbSettings(connectionString, databaseName, schema);

            var retrySchemaCreator = sqlDataProviderFactory.CreateSchemaCreator(sqlSettings);

            await retrySchemaCreator.CreateOrUpdateSchemaAsync(databaseName);
        }
}