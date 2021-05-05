namespace KafkaFlow.Retry.SqlServer
{
    using System.Threading.Tasks;

    public interface IRetrySchemaCreator
    {
        Task CreateOrUpdateSchemaAsync(string databaseName);
    }
}