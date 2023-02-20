namespace KafkaFlow.Retry.Postgres.Readers
{
    internal interface IDboDomainAdapter<TDbo, TDomain>
    {
        TDomain Adapt(TDbo dbo);
    }
}
