using System;
using Microsoft.Data.SqlClient;

namespace KafkaFlow.Retry.SqlServer;

internal interface IDbConnection : IDisposable
{
    string Schema { get; }

    SqlCommand CreateCommand();
}