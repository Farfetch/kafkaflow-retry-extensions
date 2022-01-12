namespace KafkaFlow.Retry.IntegrationTests.Core.Bootstrappers.Fixtures
{
    using System;
    using System.IO;
    using System.Threading;
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Hosting;
    using KafkaFlow.Retry.IntegrationTests.Core.Producers;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Assertion;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;
    using Xunit;

    [CollectionDefinition("BootstrapperHostCollection")]
    public class BootstrapperHostCollectionFixture : ICollectionFixture<BootstrapperHostFixture>
    { }

    public class BootstrapperHostFixture : BootstrapperFixtureTemplate
    {
        private readonly IKafkaBus kafkaBus;

        public BootstrapperHostFixture()
        {
            var config = new ConfigurationBuilder()
              .AddJsonFile(ConfigurationFilePath)
              .Build();

            this.InitializeDatabasesAsync(config).GetAwaiter().GetResult();

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
            kafkaBus = host.Services.CreateKafkaBus();
            kafkaBus.StartAsync().GetAwaiter().GetResult();

            // Wait partition assignment
            Thread.Sleep(10000);

            ServiceProvider = host.Services;
        }

        public IServiceProvider ServiceProvider { get; private set; }

        public override void Dispose()
        {
            kafkaBus.StopAsync().GetAwaiter().GetResult();

            var repositories = this.RepositoryProvider.GetAllRepositories();

            foreach (var repository in repositories)
            {
                repository.CleanDatabaseAsync().GetAwaiter().GetResult();
            }
        }

        private void SetupServices(HostBuilderContext context, IServiceCollection services)
        {
            this.InitializeKafkaAsync(context.Configuration).GetAwaiter().GetResult();

            services.AddKafka(
                kafka => kafka
                    .UseLogHandler<TraceLogHandler>()
                    .AddCluster(
                        cluster => cluster
                            .WithBrokers(this.KafkaSettings.Brokers.Split(';'))
                            .SetupRetrySimpleCluster()
                            .SetupRetryForeverCluster()
                            .SetupRetryDurableGuaranteeOrderedConsumptionMongoDbCluster(
                                this.MongoDbSettings.ConnectionString,
                                this.MongoDbSettings.DatabaseName,
                                this.MongoDbSettings.RetryQueueCollectionName,
                                this.MongoDbSettings.RetryQueueItemCollectionName)
                            .SetupRetryDurableGuaranteeOrderedConsumptionSqlServerCluster(
                                this.SqlServerSettings.ConnectionString,
                                this.SqlServerSettings.DatabaseName)
                            .SetupRetryDurableLatestConsumptionMongoDbCluster(
                                this.MongoDbSettings.ConnectionString,
                                this.MongoDbSettings.DatabaseName,
                                this.MongoDbSettings.RetryQueueCollectionName,
                                this.MongoDbSettings.RetryQueueItemCollectionName)
                            .SetupRetryDurableLatestConsumptionSqlServerCluster(
                                this.SqlServerSettings.ConnectionString,
                                this.SqlServerSettings.DatabaseName)
                    ));

            services.AddSingleton<RetrySimpleProducer>();
            services.AddSingleton<RetryForeverProducer>();
            services.AddSingleton<RetryDurableGuaranteeOrderedConsumptionMongoDbProducer>();
            services.AddSingleton<RetryDurableGuaranteeOrderedConsumptionSqlServerProducer>();
            services.AddSingleton<RetryDurableLatestConsumptionMongoDbProducer>();
            services.AddSingleton<RetryDurableLatestConsumptionSqlServerProducer>();
            services.AddSingleton<IRepositoryProvider>(sp => this.RepositoryProvider);
            services.AddSingleton<RetryDurableLatestConsumptionPhysicalStorageAssert>();
            services.AddSingleton<RetryDurableGuaranteeOrderedConsumptionPhysicalStorageAssert>();
        }
    }
}