namespace KafkaFlow.Retry.Postgres
{
    using System.Threading.Tasks;

    public interface IRetrySchemaCreator
    {
        Task CreateOrUpdateSchemaAsync(string databaseName);
    }
}