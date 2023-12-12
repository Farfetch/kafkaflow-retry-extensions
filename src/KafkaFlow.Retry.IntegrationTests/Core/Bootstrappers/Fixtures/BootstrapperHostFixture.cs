using System;
using System.IO;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using KafkaFlow.Retry.IntegrationTests.Core.Producers;
using KafkaFlow.Retry.IntegrationTests.Core.Storages.Assertion;
using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;

namespace KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures;

[CollectionDefinition("BootstrapperHostCollection")]
public class BootstrapperHostCollectionFixture : ICollectionFixture<BootstrapperHostFixture>
{ }

public class BootstrapperHostFixture : BootstrapperFixtureTemplate
{
    private readonly IKafkaBus _kafkaBus;

    public BootstrapperHostFixture()
    {
        var config = new ConfigurationBuilder()
            .AddJsonFile(ConfigurationFilePath)
            .Build();

        InitializeDatabasesAsync(config).GetAwaiter().GetResult();

        var builder = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(
                (_, config) =>
                {
                    config
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile(
                            ConfigurationFilePath,
                            false,
                            true)
                        .AddEnvironmentVariables();
                })
            .ConfigureServices(SetupServices)
            .UseDefaultServiceProvider(
                (_, options) =>
                {
                    options.ValidateScopes = true;
                    options.ValidateOnBuild = true;
                });

        var host = builder.Build();
        _kafkaBus = host.Services.CreateKafkaBus();
        _kafkaBus.StartAsync().GetAwaiter().GetResult();

        // Wait partition assignment
        Thread.Sleep(10000);

        ServiceProvider = host.Services;
    }

    public IServiceProvider ServiceProvider { get; private set; }

    public override void Dispose()
    {
        _kafkaBus.StopAsync().GetAwaiter().GetResult();

        var repositories = RepositoryProvider.GetAllRepositories();

        foreach (var repository in repositories)
        {
            repository.CleanDatabaseAsync().GetAwaiter().GetResult();
        }
    }

    private void SetupServices(HostBuilderContext context, IServiceCollection services)
    {
        InitializeKafka(context.Configuration);

        services.AddKafka(
            kafka => kafka
                .UseLogHandler<TraceLogHandler>()
                .AddCluster(
                    cluster => cluster
                        .WithBrokers(KafkaSettings.Brokers.Split(';'))
                        .CreatAllTestTopicsIfNotExist()
                        .SetupRetrySimpleCluster()
                        .SetupRetryForeverCluster()
                        .SetupRetryDurableGuaranteeOrderedConsumptionMongoDbCluster(
                            MongoDbSettings.ConnectionString,
                            MongoDbSettings.DatabaseName,
                            MongoDbSettings.RetryQueueCollectionName,
                            MongoDbSettings.RetryQueueItemCollectionName)
                        .SetupRetryDurableGuaranteeOrderedConsumptionSqlServerCluster(
                            SqlServerSettings.ConnectionString,
                            SqlServerSettings.DatabaseName)
                        .SetupRetryDurableGuaranteeOrderedConsumptionPostgresCluster(
                            PostgresSettings.ConnectionString,
                            PostgresSettings.DatabaseName)
                        .SetupRetryDurableLatestConsumptionMongoDbCluster(
                            MongoDbSettings.ConnectionString,
                            MongoDbSettings.DatabaseName,
                            MongoDbSettings.RetryQueueCollectionName,
                            MongoDbSettings.RetryQueueItemCollectionName)
                        .SetupRetryDurableLatestConsumptionSqlServerCluster(
                            SqlServerSettings.ConnectionString,
                            SqlServerSettings.DatabaseName)
                        .SetupRetryDurableLatestConsumptionPostgresCluster(
                            PostgresSettings.ConnectionString,
                            PostgresSettings.DatabaseName)
                ));

        services.AddSingleton<RetrySimpleProducer>();
        services.AddSingleton<RetryForeverProducer>();
        services.AddSingleton<RetryDurableGuaranteeOrderedConsumptionMongoDbProducer>();
        services.AddSingleton<RetryDurableGuaranteeOrderedConsumptionSqlServerProducer>();
        services.AddSingleton<RetryDurableGuaranteeOrderedConsumptionPostgresProducer>();
        services.AddSingleton<RetryDurableLatestConsumptionMongoDbProducer>();
        services.AddSingleton<RetryDurableLatestConsumptionSqlServerProducer>();
        services.AddSingleton<RetryDurableLatestConsumptionPostgresProducer>();
        services.AddSingleton<IRepositoryProvider>(sp => RepositoryProvider);
        services.AddSingleton<RetryDurableLatestConsumptionPhysicalStorageAssert>();
        services.AddSingleton<RetryDurableGuaranteeOrderedConsumptionPhysicalStorageAssert>();
    }
}