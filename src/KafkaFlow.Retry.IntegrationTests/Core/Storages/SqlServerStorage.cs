namespace KafkaFlow.Retry.IntegrationTests.Core.Storages
{
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using KafkaFlow.Retry.IntegrationTests.Core.Messages;

    internal class SqlServerStorage : IStorage
    {
        private readonly string connectionString;
        private readonly string dbName;
        private SqlConnection connection;

        public SqlServerStorage(
            string connectionString,
            string dbName)
        {
            this.connectionString = connectionString;
            this.dbName = dbName;
        }

        public Task AssertRetryDurableMessageCreationAsync(RetryDurableTestMessage message, int count)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public Task AssertRetryDurableMessageDoneAsync(RetryDurableTestMessage message)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public Task AssertRetryDurableMessageRetryingAsync(RetryDurableTestMessage message, int retryCount)
        {
            //throw new NotImplementedException();
            return Task.CompletedTask;
        }

        public void CleanDatabase()
        {
            using (SqlConnection connection = new SqlConnection(this.connectionString))
            {
                connection.Open();

                string sql = @"
                    delete from [dbo].[RetryItemMessageHeaders];
                    delete from [dbo].[ItemMessages];
                    delete from [dbo].[RetryQueues];
                    delete from [dbo].[RetryQueueItems];
                ";

                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        private void ExecuteQuery()
        {
        }
    }
}