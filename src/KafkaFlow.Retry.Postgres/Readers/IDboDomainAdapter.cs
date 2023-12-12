namespace KafkaFlow.Retry.Postgres.Readers;

internal interface IDboDomainAdapter<in TDbo, out TDomain>
{
    TDomain Adapt(TDbo dbo);
}