namespace KafkaFlow.Retry.SqlServer
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using Dawn;
    using KafkaFlow.Retry.Durable.Repository;
    using KafkaFlow.Retry.SqlServer.Model.Factories;
    using KafkaFlow.Retry.SqlServer.Model.Schema;
    using KafkaFlow.Retry.SqlServer.Readers;
    using KafkaFlow.Retry.SqlServer.Readers.Adapters;
    using KafkaFlow.Retry.SqlServer.Repositories;

    public sealed class SqlServerDbDataProviderFactory
    {
        public IKafkaRetryDurableQueueRepositoryProvider Create(SqlServerDbSettings sqlServerDbSettings)
        {
            Guard.Argument(sqlServerDbSettings)
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
                sqlServerDbSettings,
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

        public IRetrySchemaCreator CreateSchemaCreator(SqlServerDbSettings sqlServerDbSettings) => new RetrySchemaCreator(sqlServerDbSettings, this.GetScriptsForSchemaCreation());

        private IEnumerable<Script> GetScriptsForSchemaCreation()
        {
            Assembly thisAssembly = Assembly.GetExecutingAssembly();

            Script createTables = null;
            Script populateTables = null;

            using (Stream s = thisAssembly.GetManifestResourceStream("KafkaFlow.Retry.SqlServer.Deploy.01 - Create_Tables.sql"))
            {
                using (StreamReader sr = new StreamReader(s))
                {
                    createTables = new Script(sr.ReadToEnd());
                }
            }

            using (Stream s = thisAssembly.GetManifestResourceStream("KafkaFlow.Retry.SqlServer.Deploy.02 - Populate_Tables.sql"))
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