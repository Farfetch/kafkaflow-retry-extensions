namespace KafkaFlow.Retry.SqlServer
{
    internal interface IDbConnectionWithinTransaction : IDbConnection
    {
        void Commit();

        void Rollback();
    }
}