using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Dawn;
using global::Microsoft.Extensions.Configuration;
using Npgsql;
using KafkaFlow.Retry.IntegrationTests.Core.Settings;
using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

namespace KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;

public abstract class BootstrapperFixtureTemplate : IDisposable
{
    protected const string ConfigurationFilePath = "conf/appsettings.json";

    private bool databasesInitialized;

    private IRepositoryProvider repositoryProvider;

    internal KafkaSettings KafkaSettings { get; private set; }

    internal MongoDbRepositorySettings MongoDbSettings { get; private set; }

    internal IRepositoryProvider RepositoryProvider => this.repositoryProvider ?? this.CreateRepositoryProvider();

    internal SqlServerRepositorySettings SqlServerSettings { get; private set; }
      
    internal PostgresRepositorySettings PostgresSettings { get; private set; }

    public abstract void Dispose();

    protected async Task InitializeDatabasesAsync(IConfiguration configuration)
    {
        this.InitializeMongoDb(configuration);
        await this.InitializeSqlServerAsync(configuration).ConfigureAwait(false);
        await this.InitializePostgresAsync(configuration).ConfigureAwait(false);

        this.databasesInitialized = true;
    }

    protected void InitializeKafka(IConfiguration configuration)
    {
        this.KafkaSettings = configuration.GetSection("Kafka").Get<KafkaSettings>();
    }

    private IRepositoryProvider CreateRepositoryProvider()
    {
        Guard.Argument(this.databasesInitialized, nameof(this.databasesInitialized)).True($"Call {nameof(this.InitializeDatabasesAsync)} first.");

        var repositories = new List<IRepository>
        {
            new MongoDbRepository( this.MongoDbSettings.ConnectionString, this.MongoDbSettings.DatabaseName, this.MongoDbSettings.RetryQueueCollectionName, this.MongoDbSettings.RetryQueueItemCollectionName),
            new SqlServerRepository(this.SqlServerSettings.ConnectionString, this.SqlServerSettings.DatabaseName),
            new PostgresRepository(this.PostgresSettings.ConnectionString, this.PostgresSettings.DatabaseName)
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

    private async Task InitializePostgresAsync(IConfiguration configuration)
    {
        this.PostgresSettings = configuration.GetSection("PostgresRepository").Get<PostgresRepositorySettings>();

        var postgresConnectionStringBuilder = new NpgsqlConnectionStringBuilder(this.PostgresSettings.ConnectionString);
        this.PostgresSettings.ConnectionString = postgresConnectionStringBuilder.ToString();

        await BootstrapperPostgresSchema.RecreatePostgresSchemaAsync(this.PostgresSettings.DatabaseName, this.PostgresSettings.ConnectionString).ConfigureAwait(false);
    }
}