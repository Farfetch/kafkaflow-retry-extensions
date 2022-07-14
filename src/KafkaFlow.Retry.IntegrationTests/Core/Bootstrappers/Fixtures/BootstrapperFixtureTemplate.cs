namespace KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Dawn;
    using global::Microsoft.Extensions.Configuration;
    using KafkaFlow.Retry.IntegrationTests.Core.Settings;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

    public abstract class BootstrapperFixtureTemplate : IDisposable
    {
        protected const string ConfigurationFilePath = "conf/appsettings.json";

        private bool databasesInitialized;

        private IRepositoryProvider repositoryProvider;

        internal KafkaSettings KafkaSettings { get; private set; }

        internal MongoDbRepositorySettings MongoDbSettings { get; private set; }

        internal IRepositoryProvider RepositoryProvider => this.repositoryProvider ?? this.CreateRepositoryProvider();

        internal SqlServerRepositorySettings SqlServerSettings { get; private set; }

        public abstract void Dispose();

        protected async Task InitializeDatabasesAsync(IConfiguration configuration)
        {
            this.InitializeMongoDb(configuration);
            await this.InitializeSqlServerAsync(configuration).ConfigureAwait(false);

            this.databasesInitialized = true;
        }

        protected async Task InitializeKafkaAsync(IConfiguration configuration)
        {
            this.KafkaSettings = configuration.GetSection("Kafka").Get<KafkaSettings>();

            var topics = new[]
           {
                "test-kafka-flow-retry-retry-simple",
                "test-kafka-flow-retry-retry-forever",
                "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-mongo-db",
                "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-mongo-db-retry",
                "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-sql-server",
                "test-kafka-flow-retry-retry-durable-guarantee-ordered-consumption-sql-server-retry",
                "test-kafka-flow-retry-retry-durable-latest-consumption-mongo-db",
                "test-kafka-flow-retry-retry-durable-latest-consumption-mongo-db-retry",
                "test-kafka-flow-retry-retry-durable-latest-consumption-sql-server",
                "test-kafka-flow-retry-retry-durable-latest-consumption-sql-server-retry",

                "test-kafka-flow-retry-empty-retry-durable-guarantee-ordered-consumption-mongo-db",
                "test-kafka-flow-retry-empty-retry-durable-guarantee-ordered-consumption-mongo-db-retry",
                "test-kafka-flow-retry-empty-retry-durable-guarantee-ordered-consumption-sql-server",
                "test-kafka-flow-retry-empty-retry-durable-guarantee-ordered-consumption-sql-server-retry",
                "test-kafka-flow-retry-empty-retry-durable-latest-consumption-mongo-db",
                "test-kafka-flow-retry-empty-retry-durable-latest-consumption-mongo-db-retry",
                "test-kafka-flow-retry-empty-retry-durable-latest-consumption-sql-server",
                "test-kafka-flow-retry-empty-retry-durable-latest-consumption-sql-server-retry",

                "test-kafka-flow-retry-null-retry-durable-guarantee-ordered-consumption-mongo-db",
                "test-kafka-flow-retry-null-retry-durable-guarantee-ordered-consumption-mongo-db-retry",
                "test-kafka-flow-retry-null-retry-durable-guarantee-ordered-consumption-sql-server",
                "test-kafka-flow-retry-null-retry-durable-guarantee-ordered-consumption-sql-server-retry",
                "test-kafka-flow-retry-null-retry-durable-latest-consumption-mongo-db",
                "test-kafka-flow-retry-null-retry-durable-latest-consumption-mongo-db-retry",
                "test-kafka-flow-retry-null-retry-durable-latest-consumption-sql-server",
                "test-kafka-flow-retry-null-retry-durable-latest-consumption-sql-server-retry"
            };

            await BootstrapperKafka.RecreateKafkaTopicsAsync(this.KafkaSettings.Brokers, topics);
        }

        private IRepositoryProvider CreateRepositoryProvider()
        {
            Guard.Argument(this.databasesInitialized, nameof(this.databasesInitialized)).True($"Call {nameof(this.InitializeDatabasesAsync)} first.");

            var repositories = new List<IRepository>
            {
                new MongoDbRepository( this.MongoDbSettings.ConnectionString, this.MongoDbSettings.DatabaseName, this.MongoDbSettings.RetryQueueCollectionName, this.MongoDbSettings.RetryQueueItemCollectionName),
                new SqlServerRepository(this.SqlServerSettings.ConnectionString, this.SqlServerSettings.DatabaseName)
            };

            this.repositoryProvider = new RepositoryProvider(repositories);

            return this.repositoryProvider;
        }

        private void InitializeMongoDb(IConfiguration configuration)
        {
            this.MongoDbSettings = configuration.GetSection("MongoDbRepository").Get<MongoDbRepositorySettings>();
        }

        private async Task InitializeSqlServerAsync(IConfiguration configuration)
        {
            this.SqlServerSettings = configuration.GetSection("SqlServerRepository").Get<SqlServerRepositorySettings>();

            var sqlServerConnectionStringBuilder = new SqlConnectionStringBuilder(this.SqlServerSettings.ConnectionString);
            if (Environment.GetEnvironmentVariable("SQLSERVER_INTEGRATED_SECURITY") != null)
            {
                sqlServerConnectionStringBuilder.IntegratedSecurity = false;
            }
            this.SqlServerSettings.ConnectionString = sqlServerConnectionStringBuilder.ToString();

            await BootstrapperSqlServerSchema.RecreateSqlSchemaAsync(this.SqlServerSettings.DatabaseName, this.SqlServerSettings.ConnectionString).ConfigureAwait(false);
        }
    }
}