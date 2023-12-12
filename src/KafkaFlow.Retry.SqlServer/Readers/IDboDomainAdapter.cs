namespace KafkaFlow.Retry.SqlServer.Readers;

internal interface IDboDomainAdapter<TDbo, TDomain>
{
    TDomain Adapt(TDbo dbo);
}