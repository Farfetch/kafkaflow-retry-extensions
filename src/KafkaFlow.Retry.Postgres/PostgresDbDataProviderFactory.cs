namespace KafkaFlow.Retry.Postgres
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.Postgres.Model.Factories;
    using KafkaFlow.Retry.Postgres.Model.Schema;
    using KafkaFlow.Retry.Postgres.Readers;
    using KafkaFlow.Retry.Postgres.Readers.Adapters;
    using KafkaFlow.Retry.Postgres.Repositories;
    
    public sealed class PostgresDbDataProviderFactory
    {
        public IRetryDurableQueueRepositoryProvider Create(PostgresDbSettings postgresDbSettings)
        {
            Guard.Argument(postgresDbSettings)
                .NotNull("It is mandatory to config the factory before creating new instances of IRetryQueueDataProvider. Make sure the Config method is executed before the Create method.");

            var retryQueueItemMessageAdapter =
                new RetryQueueItemMessageDboFactory();

            var retryQueueReader = new RetryQueueReader(
                new RetryQueueAdapter(),
                new RetryQueueItemAdapter(),
                new RetryQueueItemMessageAdapter(),
                new RetryQueueItemMessageHeaderAdapter()
                );

            return new RetryQueueDataProvider(
                postgresDbSettings,
                new ConnectionProvider(),
                new RetryQueueItemMessageHeaderRepository(),
                new RetryQueueItemMessageRepository(),
                new RetryQueueItemRepository(),
                new RetryQueueRepository(),
                new RetryQueueDboFactory(),
                new RetryQueueItemDboFactory(),
                retryQueueReader,
                retryQueueItemMessageAdapter,
                new RetryQueueItemMessageHeaderDboFactory());
        }

        public IRetrySchemaCreator CreateSchemaCreator(PostgresDbSettings postgresDbSettings) => new RetrySchemaCreator(postgresDbSettings, this.GetScriptsForSchemaCreation());

        private IEnumerable<Script> GetScriptsForSchemaCreation()
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();

            Script createTables = null;
            Script populateTables = null;

            using (Stream s = thisAssembly.GetManifestResourceStream("KafkaFlow.Retry.Postgres.Deploy.01 - Create_Tables.sql"))
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    createTables = new Script(sr.ReadToEnd());
                }
            }

            using (Stream s = thisAssembly.GetManifestResourceStream("KafkaFlow.Retry.Postgres.Deploy.02 - Populate_Tables.sql"))
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    populateTables = new Script(sr.ReadToEnd());
                }
            }

            return new[] { createTables, populateTables };
        }
    }
}