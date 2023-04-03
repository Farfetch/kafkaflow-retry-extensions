namespace KafkaFlow.Retry.Postgres
{
    using System;
    using Npgsql;
    
    internal interface IDbConnection : IDisposable
    {
        NpgsqlCommand CreateCommand();
    }
}