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
        /// <param name="resolve">A function that, given one or more entities, resolves to an entity instance to which child entities will be added.</param>
        /// <returns>A Dapper mapping function.</returns>
        public Func<IEnumerable<object>, TEntityType> Build<TEntityType>(Func<TEntityType, TEntityType, TEntityType> resolve = null)
            where TEntityType : class
        {
            if (resolve == null)
            {
                // A non-resolver, always resolves to the next object provided.
                resolve = (current, next) => next;
            }

            // Build an entity mapper for the given type
            var mapper = serviceProvider.GetService(typeof(IEntityMapper<TEntityType>)) as IEntityMapper<TEntityType>;
            if (mapper == null)
            {
                throw new InvalidOperationException($"Could not find a mapper for type {typeof(IEntityMapper<TEntityType>).Name}");
            }

            TEntityType entity = null;

            // Setup the mapper to properly resolve its entity
            mapper.ResolveEntity = e => resolve(entity, e);

            return objs =>
            {
                // Map the object
                var next = mapper.Map(objs);

                // Return null if we are returning a duplicate object
                if (object.ReferenceEquals(next, entity))
                {
                    return null;
                }

                // Save a reference to the entity
                entity = next;

                // And, return it
                return entity;
            };
        }

        /// <summary>
        /// Builds an entity mapper for the given entity type.
        /// </summary>
        /// <typeparam name="TEntityType">The type of entity to be mapped.</typeparam>
        /// <param name="resolve">A function that compares two values on the entity for equality, usually comparing primary keys.</param>
        /// <returns>A Dapper mapping function.</returns>
        public Func<IEnumerable<object>, TEntityType> Build<TEntityType>(Func<TEntityType, object> selector)
            where TEntityType : class
        {
            var resolve = new Func<TEntityType, TEntityType, TEntityType>(
                (current, next) =>
                {
                    TEntityType result = next;
                    if (current != null)
                    {
                        // Compare against values on the objects (usually a primary key)
                        result = object.Equals(selector(current), selector(next)) ? current : next;
                    }
                    return result;
                }
            );

            // Build the mapper
            return Build(resolve);
        }
    }
}