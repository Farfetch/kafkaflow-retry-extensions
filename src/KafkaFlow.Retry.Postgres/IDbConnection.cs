using System;
using Npgsql;

namespace KafkaFlow.Retry.Postgres;

internal interface IDbConnection : IDisposable
{
    NpgsqlCommand CreateCommand();
}