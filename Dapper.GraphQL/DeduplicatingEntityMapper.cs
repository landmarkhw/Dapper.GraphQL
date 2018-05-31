using GraphQL.Language.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dapper.GraphQL
{
    /// <summary>
    /// A wrapper for an entity mapper.
    /// </summary>
    public abstract class DeduplicatingEntityMapper<TEntityType> :
        IEntityMapper<TEntityType>
        where TEntityType : class
    {
        /// <summary>
        /// The entity mapper.
        /// </summary>
        public IEntityMapper<TEntityType> Mapper { get; set; }

        /// <summary>
        /// Sets a function that returns the primary key used to uniquely identify the entity.
        /// </summary>
        public Func<TEntityType, object> PrimaryKey { get; set; }

        /// <summary>
        /// True if this mapper should return null when duplicates are encountered.
        /// </summary>
        public bool ReturnsNullWithDuplicates { get; set; } = true;

        /// <summary>
        /// A cache used to hold previous entities that this mapper has seen.
        /// </summary>
        protected IDictionary<object, TEntityType> KeyCache { get; set; } = new Dictionary<object, TEntityType>();

        /// <summary>
        /// Maps a row of data to an entity.
        /// </summary>
        /// <param name="context">A context that contains information used to map Dapper objects.</param>
        /// <returns>The mapped entity, or null if the entity has previously been returned.</returns>
        public abstract TEntityType Map(EntityMapContext context);

        /// <summary>
        /// Resolves the deduplicated entity.
        /// </summary>
        /// <param name="entity">The entity to deduplicate.</param>
        /// <returns>The deduplicated entity.</returns>
        protected virtual TEntityType Deduplicate(TEntityType entity)
        {
            if (entity == default(TEntityType))
            {
                return default(TEntityType);
            }

            if (PrimaryKey == null)
            {
                throw new InvalidOperationException("PrimaryKey selector is not defined, but is required to use DeduplicatingEntityMapper.");
            }

            var previous = entity;

            var primaryKey = PrimaryKey(entity);
            if (primaryKey == null)
            {
                throw new InvalidOperationException("A null primary key was provided, which results in an unpredictable state.");
            }

            // Deduplicate the entity using available information
            if (KeyCache.ContainsKey(primaryKey))
            {
                if (ReturnsNullWithDuplicates)
                {
                    return null;
                }
                entity = KeyCache[primaryKey];
            }
            else
            {
                // Cache a reference to the entity
                KeyCache[primaryKey] = entity;
            }

            return entity;
        }
    }
}