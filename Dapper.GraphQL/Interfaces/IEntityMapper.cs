using GraphQL.Language.AST;
using System;
using System.Collections.Generic;

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
        /// <param name="objs">A row objects to be mapped.</param>
        /// <param name="selectionSet">The GraphQL selection set, or null if none was provided.</param>
        /// <param name="splitOn">The types the query is split on.</param>
        /// <returns>The mapped entity, or null if the entity has previously been returned.</returns>
        TEntityType Map(object[] objs, IHaveSelectionSet selectionSet, List<Type> splitOn);
    }
}