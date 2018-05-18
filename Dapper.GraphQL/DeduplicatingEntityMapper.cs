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
    public class DeduplicatingEntityMapper<TEntityType> :
        IEntityMapper<TEntityType>
        where TEntityType : class
    {
        /// <summary>
        /// A cache used to hold previous entities that this mapper has seen.
        /// </summary>
        private IDictionary<object, TEntityType> KeyCache { get; set; } = new Dictionary<object, TEntityType>();

        /// <summary>
        /// Sets a function that returns the primary key used to uniquely identify the entity.
        /// </summary>
        public Func<TEntityType, object> PrimaryKey { get; set; }

        /// <summary>
        /// The entity mapper.
        /// </summary>
        public IEntityMapper<TEntityType> Mapper { get; set; }

        /// <summary>
        /// Maps a row of data to an entity.
        /// </summary>
        /// <param name="context">A context that contains information used to map Dapper objects.</param>
        /// <returns>The mapped entity, or null if the entity has previously been returned.</returns>
        public virtual TEntityType Map(EntityMapContext context)
        {
            if (PrimaryKey == null)
            {
                throw new InvalidOperationException("PrimaryKey selector is not defined, but is required to use DeduplicatingEntityMapper.");
            }

            // Deduplicate the top object (entity) in the list
            if (context.Items != null &&
                context.Items.Any())
            {
                if (context.Items.First() is TEntityType entity)
                {
                    var previous = entity;

                    var primaryKey = PrimaryKey(entity);
                    if (primaryKey == null)
                    {
                        throw new InvalidOperationException("A null primary key was provided, which results in an unpredictable state.");
                    }

                    if (KeyCache.ContainsKey(primaryKey))
                    {
                        entity = KeyCache[primaryKey];
                        if (!object.ReferenceEquals(previous, entity))
                        {
                            context.Items = new[] { entity }.Concat(context.Items.Skip(1));
                        }
                    }

                    // Map the object
                    var next = Mapper.Map(context);
                    
                    // Return null if we are returning a duplicate object.
                    // Queries can filter out null entries to prevent duplicates.
                    if (KeyCache.ContainsKey(primaryKey))
                    {
                        return null;
                    }

                    // Cache a reference to the entity
                    KeyCache[primaryKey] = next;

                    // And, return it
                    return next;
                }
            }

            return default(TEntityType);
        }
    }
}
