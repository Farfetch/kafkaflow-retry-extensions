namespace KafkaFlow.Retry.SqlServer
{
    using System;
    using System.Data.SqlClient;

    internal interface IDbConnection : IDisposable
    {
        SqlCommand CreateCommand();
    }
}