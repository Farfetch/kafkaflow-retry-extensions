using System.Threading.Tasks;

namespace KafkaFlow.Retry.Postgres
{
    public interface IRetrySchemaCreator
    {
        Task CreateOrUpdateSchemaAsync(string databaseName);
    }
}