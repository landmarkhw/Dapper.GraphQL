namespace Dapper.GraphQL
{
    /// <summary>
    /// Maps a row of objects from Dapper into an entity.
    /// </summary>
    /// <typeparam name="TEntityType">The type of entity to be mapped.</typeparam>
    public interface IEntityMapper<TEntityType> where TEntityType : class
    {
        /// <summary>
        /// Maps a row of data to an entity.
        /// </summary>
        /// <param name="context">A context that contains information used to map Dapper objects.</param>
        /// <returns>The mapped entity, or null if the entity has previously been returned.</returns>
        TEntityType Map(EntityMapContext context);
    }
}