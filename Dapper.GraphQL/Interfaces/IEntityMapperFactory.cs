using GraphQL.Language.AST;
using System;
using System.Collections.Generic;

namespace Dapper.GraphQL
{
    /// <summary>
    /// A factory that creates Dapper entity mappers.
    /// </summary>
    public interface IEntityMapperFactory
    {
        /// <summary>
        /// Builds an entity mapper for the given entity type.
        /// </summary>
        /// <typeparam name="TEntityType">The type of entity to be mapped.</typeparam>
        /// <param name="deduplicate">A function that, given one or more entities, resolves to an entity instance to which child entities will be added.</param>
        /// <returns>A Dapper mapping function.</returns>
        IEntityMapper<TEntityType> Build<TEntityType>(
            Func<TEntityType, TEntityType, TEntityType> deduplicate = null)
            where TEntityType : class;

        /// <summary>
        /// Builds an entity mapper for the given entity type.
        /// </summary>
        /// <typeparam name="TEntityType">The type of entity to be mapped.</typeparam>
        /// <param name="resolve">A function that compares two values on the entity for equality, usually comparing primary keys.</param>        
        /// <returns>A Dapper mapping function.</returns>
        IEntityMapper<TEntityType> Build<TEntityType>(
            Func<TEntityType, object> selector)
            where TEntityType : class;
    }
}