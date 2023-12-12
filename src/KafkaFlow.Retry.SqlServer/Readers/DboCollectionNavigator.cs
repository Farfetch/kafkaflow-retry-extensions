using System;
using System.Collections.Generic;
using Dawn;

namespace KafkaFlow.Retry.SqlServer.Readers;

internal class DboCollectionNavigator<TDbo, TDomain> where TDbo : class
{
    private readonly IDboDomainAdapter<TDbo, TDomain> _dboDomainAdapter;
    private readonly IList<TDbo> _dbos;
    private int _currentIndex;

    public DboCollectionNavigator(IList<TDbo> dbos, IDboDomainAdapter<TDbo, TDomain> dboDomainAdapter)
    {
        Guard.Argument(dbos, nameof(dbos)).NotNull();
        Guard.Argument(dboDomainAdapter, nameof(dboDomainAdapter)).NotNull();

        _dboDomainAdapter = dboDomainAdapter;
        _dbos = dbos;
    }

    public void Navigate(Action<TDomain> action, Predicate<TDbo> navigatingCondition)
    {
        Guard.Argument(action).NotNull();
        Guard.Argument(navigatingCondition).NotNull();

        Navigate((domain, dbo) => action(domain), navigatingCondition);
    }

    public void Navigate(Action<TDomain, TDbo> action, Predicate<TDbo> navigatingCondition)
    {
        Guard.Argument(action).NotNull();
        Guard.Argument(navigatingCondition).NotNull();

        while (_currentIndex < _dbos.Count)
        {
            var currentDbo = _dbos[_currentIndex];

            if (!navigatingCondition(currentDbo))
            {
                return;
            }

            action(_dboDomainAdapter.Adapt(currentDbo), currentDbo);

            _currentIndex++;
        }
    }
}