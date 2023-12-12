using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Configuration;
using Npgsql;
using KafkaFlow.Retry.IntegrationTests.Core.Settings;
using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

namespace KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;

public abstract class BootstrapperFixtureTemplate : IDisposable
{
    protected const string ConfigurationFilePath = "conf/appsettings.json";

    private bool _databasesInitialized;

    private IRepositoryProvider _repositoryProvider;

    internal KafkaSettings KafkaSettings { get; private set; }

    internal MongoDbRepositorySettings MongoDbSettings { get; private set; }

    internal IRepositoryProvider RepositoryProvider => _repositoryProvider ?? CreateRepositoryProvider();

    internal SqlServerRepositorySettings SqlServerSettings { get; private set; }
      
    internal PostgresRepositorySettings PostgresSettings { get; private set; }

    public abstract void Dispose();

    protected async Task InitializeDatabasesAsync(IConfiguration configuration)
    {
        InitializeMongoDb(configuration);
        await InitializeSqlServerAsync(configuration).ConfigureAwait(false);
        await InitializePostgresAsync(configuration).ConfigureAwait(false);

        _databasesInitialized = true;
    }

    protected void InitializeKafka(IConfiguration configuration)
    {
        KafkaSettings = configuration.GetSection("Kafka").Get<KafkaSettings>();
    }

    private IRepositoryProvider CreateRepositoryProvider()
    {
        Guard.Argument(_databasesInitialized, nameof(_databasesInitialized)).True($"Call {nameof(InitializeDatabasesAsync)} first.");

        var repositories = new List<IRepository>
        {
            new MongoDbRepository( MongoDbSettings.ConnectionString, MongoDbSettings.DatabaseName, MongoDbSettings.RetryQueueCollectionName, MongoDbSettings.RetryQueueItemCollectionName),
            new SqlServerRepository(SqlServerSettings.ConnectionString, SqlServerSettings.DatabaseName),
            new PostgresRepository(PostgresSettings.ConnectionString, PostgresSettings.DatabaseName)
        };

        _repositoryProvider = new RepositoryProvider(repositories);

        return _repositoryProvider;
    }

    private void InitializeMongoDb(IConfiguration configuration)
    {
        MongoDbSettings = configuration.GetSection("MongoDbRepository").Get<MongoDbRepositorySettings>();
    }

    private async Task InitializeSqlServerAsync(IConfiguration configuration)
    {
        SqlServerSettings = configuration.GetSection("SqlServerRepository").Get<SqlServerRepositorySettings>();

        var sqlServerConnectionStringBuilder = new SqlConnectionStringBuilder(SqlServerSettings.ConnectionString);
        if (Environment.GetEnvironmentVariable("SQLSERVER_INTEGRATED_SECURITY") != null)
        {
            sqlServerConnectionStringBuilder.IntegratedSecurity = false;
        }
        SqlServerSettings.ConnectionString = sqlServerConnectionStringBuilder.ToString();

        await BootstrapperSqlServerSchema.RecreateSqlSchemaAsync(SqlServerSettings.DatabaseName, SqlServerSettings.ConnectionString).ConfigureAwait(false);
    }

    private async Task InitializePostgresAsync(IConfiguration configuration)
    {
        PostgresSettings = configuration.GetSection("PostgresRepository").Get<PostgresRepositorySettings>();

        var postgresConnectionStringBuilder = new NpgsqlConnectionStringBuilder(PostgresSettings.ConnectionString);
        PostgresSettings.ConnectionString = postgresConnectionStringBuilder.ToString();

        await BootstrapperPostgresSchema.RecreatePostgresSchemaAsync(PostgresSettings.DatabaseName, PostgresSettings.ConnectionString).ConfigureAwait(false);
    }
}