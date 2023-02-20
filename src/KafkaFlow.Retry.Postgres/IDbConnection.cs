using System;

namespace KafkaFlow.Retry.Postgres
{
    internal interface IDbConnection : IDisposable
    {
        SqlCommand CreateCommand();
    }
}