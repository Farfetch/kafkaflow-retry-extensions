namespace KafkaFlow.Retry.Postgres;

internal interface IDbConnectionWithinTransaction : IDbConnection
{
    void Commit();

    void Rollback();
}