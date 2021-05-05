namespace KafkaFlow.Retry.MongoDb
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using MongoDB.Driver;

    [ExcludeFromCodeCoverage]
    internal static class MongoRepositoryCollectionExtensions
    {
        public static async Task<IEnumerable<TCollection>> GetAsync<TCollection>(this IMongoCollection<TCollection> collection, FilterDefinition<TCollection> filter, FindOptions<TCollection> options = null)
        {
            var data = new List<TCollection>();

            var cursor = await collection.FindAsync(filter, options).ConfigureAwait(false);

            while (await cursor.MoveNextAsync().ConfigureAwait(false))
            {
                data.AddRange(cursor.Current);
            }

            return data;
        }

        public static FilterDefinitionBuilder<TCollection> GetFilters<TCollection>(this IMongoCollection<TCollection> collection)
        {
            return Builders<TCollection>.Filter;
        }

        public static async Task<TCollection> GetOneAsync<TCollection>(this IMongoCollection<TCollection> collection, FilterDefinition<TCollection> filter)
        {
            return await collection.Find(filter).FirstOrDefaultAsync().ConfigureAwait(false);
        }

        public static SortDefinitionBuilder<TCollection> GetSortDefinition<TCollection>(this IMongoCollection<TCollection> collection)
        {
            return Builders<TCollection>.Sort;
        }

        public static UpdateDefinitionBuilder<TCollection> GetUpdateDefinition<TCollection>(this IMongoCollection<TCollection> collection)
        {
            return Builders<TCollection>.Update;
        }
    }
}
