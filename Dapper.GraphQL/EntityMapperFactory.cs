using GraphQL.Language.AST;
using System;
using System.Collections.Generic;

namespace Dapper.GraphQL
{
    /// <summary>
    /// A factory that creates Dapper entity mappers.
    /// </summary>
    public class EntityMapperFactory :
        IEntityMapperFactory
    {
        private readonly IServiceProvider serviceProvider;

        public EntityMapperFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Builds an entity mapper for the given entity type.
        /// </summary>
        /// <typeparam name="TEntityType">The type of entity to be mapped.</typeparam>
        /// <param name="deduplicate">A function that, given one or more entities, resolves to an entity instance to which child entities will be added.</param>                
        /// <param name="returnNullForDuplicates">True if duplicate entries should return null, false otherwise.</param>
        /// <returns>A Dapper mapping function.</returns>
        public IEntityMapper<TEntityType> Build<TEntityType>(
            Func<TEntityType, TEntityType, TEntityType> deduplicate = null,
            bool returnNullForDuplicates = true)
            where TEntityType : class
        {
            // Build an entity mapper for the given type
            var mapper = serviceProvider.GetService(typeof(IEntityMapper<TEntityType>)) as IEntityMapper<TEntityType>;
            if (mapper == null)
            {
                throw new InvalidOperationException($"Could not find a mapper for type {typeof(IEntityMapper<TEntityType>).Name}");
            }

            if (!returnNullForDuplicates)
            {
                return mapper;
            }

            var deduplicatingMapper = new DeduplicatingEntityMapper<TEntityType>
            {
                Mapper = mapper
            };

            if (deduplicate != null)
            {
                deduplicatingMapper.Deduplicate = deduplicate;
            }

            return deduplicatingMapper;
        }

        /// <summary>
        /// Builds an entity mapper for the given entity type.
        /// </summary>
        /// <typeparam name="TEntityType">The type of entity to be mapped.</typeparam>
        /// <param name="selector">A function that returns the primary key of an object, used to deduplicate objects.</param>
        /// <param name="returnNullForDuplicates">True if duplicate entries should return null, false otherwise.</param>
        /// <returns>A Dapper mapping function.</returns>
        public IEntityMapper<TEntityType> Build<TEntityType>(
            Func<TEntityType, object> selector,
            bool returnNullForDuplicates = true)
            where TEntityType : class
        {
            var deduplicate = new Func<TEntityType, TEntityType, TEntityType>(
                (previous, current) =>
                {
                    TEntityType result = current;
                    if (previous != null)
                    {
                        // Compare against values on the objects (usually a primary key)
                        result = object.Equals(selector(previous), selector(current)) ? previous : current;
                    }
                    return result;
                }
            );

            // Build the mapper
            return Build(deduplicate, returnNullForDuplicates);
        }
    }
}