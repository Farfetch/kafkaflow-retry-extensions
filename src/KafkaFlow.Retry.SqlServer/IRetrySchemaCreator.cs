using System.Threading.Tasks;

namespace KafkaFlow.Retry.SqlServer;

public interface IRetrySchemaCreator
{
    Task CreateOrUpdateSchemaAsync(string databaseName);
}