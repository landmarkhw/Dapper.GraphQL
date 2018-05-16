using System;
using System.Collections.Generic;
using GraphQL.Language.AST;

namespace Dapper.GraphQL
{
    /// <summary>
    /// A base class for Dapper entity mappers.
    /// </summary>
    /// <typeparam name="TEntityType">The type of entity to be mapped from Dapper query results.</typeparam>
    public abstract class EntityMapper<TEntityType> :
        IEntityMapper<TEntityType>
        where TEntityType : class
    {
        /// <summary>
        /// Maps a row of data to an entity.
        /// </summary>
        /// <param name="objs">A row objects to be mapped.</param>
        /// <param name="selectionSet">The GraphQL selection set (optional).</param>
        /// <param name="splitOn">The types the query is split on.</param>
        /// <returns>The mapped entity, or null if the entity has previously been returned.</returns>
        public abstract TEntityType Map(object[] objs, IHaveSelectionSet selectionSet, List<Type> splitOn);
    }
}