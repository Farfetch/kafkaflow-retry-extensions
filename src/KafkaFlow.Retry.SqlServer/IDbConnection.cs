namespace KafkaFlow.Retry.SqlServer
{
    using System;
    using Microsoft.Data.SqlClient;

    internal interface IDbConnection : IDisposable
    {
        string Schema { get; }

        SqlCommand CreateCommand();
    }
}