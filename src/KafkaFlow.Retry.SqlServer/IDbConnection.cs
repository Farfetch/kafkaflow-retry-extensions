namespace KafkaFlow.Retry.SqlServer
{
    using System;
    using System.Data.SqlClient;

    internal interface IDbConnection : IDisposable
    {
        string Schema { get; }

        SqlCommand CreateCommand();
    }
}