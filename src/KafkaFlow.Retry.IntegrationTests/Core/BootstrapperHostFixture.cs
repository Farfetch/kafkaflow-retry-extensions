namespace KafkaFlow.Retry.IntegrationTests.Core
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using global::Microsoft.Extensions.Configuration;
    using global::Microsoft.Extensions.DependencyInjection;
    using global::Microsoft.Extensions.Hosting;
    using KafkaFlow.Retry.IntegrationTests.Core.Producers;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages;
    using KafkaFlow.Retry.IntegrationTests.Core.Storages.Repositories;
    using Xunit;

    [CollectionDefinition("BootstrapperHostCollection")]
    public class BootstrapperHostCollectionFixture : ICollectionFixture<BootstrapperHostFixture>
    { }

    public class BootstrapperHostFixture : IDisposable
    {
        private static IKafkaBus kafkaBus;

        public BootstrapperHostFixture()
        {
            var builder = Host
                .CreateDefaultBuilder()
                .ConfigureAppConfiguration(
                    (_, config) =>
                    {
                        config
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile(
                                "conf/appsettings.json",
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

        public void Dispose()
        {
            kafkaBus.StopAsync().GetAwaiter().GetResult();
        }

        private static void SetupServices(HostBuilderContext context, IServiceCollection services)
        {
            var kafkaBrokers = context.Configuration.GetValue<string>("Kafka:Brokers");
            var mongoDbConnectionString = context.Configuration.GetValue<string>("MongoDbRepository:ConnectionString");
            var mongoDbDatabaseName = context.Configuration.GetValue<string>("MongoDbRepository:DatabaseName");
            var mongoDbRetryQueueCollectionName = context.Configuration.GetValue<string>("MongoDbRepository:RetryQueueCollectionName");
            var mongoDbRetryQueueItemCollectionName = context.Configuration.GetValue<string>("MongoDbRepository:RetryQueueItemCollectionName");
            var sqlServerConnectionString = context.Configuration.GetValue<string>("SqlServerRepository:ConnectionString");
            if (Environment.GetEnvironmentVariable("SQLSERVER_SA_PASSWORD") != null)
            {
                sqlServerConnectionString += "Integrated Security=false;";
            }
            var sqlServerDatabaseName = context.Configuration.GetValue<string>("SqlServerRepository:DatabaseName");
            var topics = new string[]
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
                "test-kafka-flow-retry-retry-durable-latest-consumption-sql-server-retry"
            };

            BootstrapperKafka.RecreateKafkaTopics(kafkaBrokers, topics).GetAwaiter().GetResult();
            BootstrapperSqlServerSchema.RecreateSqlSchema(sqlServerDatabaseName, sqlServerConnectionString).GetAwaiter().GetResult();

            services.AddKafka(
                kafka => kafka
                    .UseLogHandler<TraceLogHandler>()
                    .AddCluster(
                        cluster => cluster
                            .WithBrokers(kafkaBrokers.Split(';'))
                            .SetupRetrySimpleCluster()
                            .SetupRetryForeverCluster()
                            .SetupRetryDurableGuaranteeOrderedConsumptionMongoDbCluster(
                                mongoDbConnectionString,
                                mongoDbDatabaseName,
                                mongoDbRetryQueueCollectionName,
                                mongoDbRetryQueueItemCollectionName)
                            .SetupRetryDurableGuaranteeOrderedConsumptionSqlServerCluster(
                                sqlServerConnectionString,
                                sqlServerDatabaseName)
                            .SetupRetryDurableLatestConsumptionMongoDbCluster(
                                mongoDbConnectionString,
                                mongoDbDatabaseName,
                                mongoDbRetryQueueCollectionName,
                                mongoDbRetryQueueItemCollectionName)
                            .SetupRetryDurableLatestConsumptionSqlServerCluster(
                                sqlServerConnectionString,
                                sqlServerDatabaseName)
                    ));

            services.AddSingleton<RetrySimpleProducer>();
            services.AddSingleton<RetryForeverProducer>();
            services.AddSingleton<RetryDurableGuaranteeOrderedConsumptionMongoDbProducer>();
            services.AddSingleton<RetryDurableGuaranteeOrderedConsumptionSqlServerProducer>();
            services.AddSingleton<RetryDurableLatestConsumptionMongoDbProducer>();
            services.AddSingleton<RetryDurableLatestConsumptionSqlServerProducer>();
            services.AddSingleton<IRepositoryProvider>(
                sp => new RepositoryProvider(
                    new List<IRepository>
                    {
                        new MongoDbRepository(mongoDbConnectionString, mongoDbDatabaseName, mongoDbRetryQueueCollectionName, mongoDbRetryQueueItemCollectionName),
                        new SqlServerRepository(sqlServerConnectionString, sqlServerDatabaseName)
                    })
                );
            services.AddSingleton<RetryDurableLatestConsumptionPhysicalStorageAssert>();
            services.AddSingleton<RetryDurableGuaranteeOrderedConsumptionPhysicalStorageAssert>();
        }
    }
}