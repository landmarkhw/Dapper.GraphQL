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
        /// Deduplicates the entity, comparing it to the previous entity that was seen by the entity mapper.
        /// </summary>
        public Func<TEntityType, TEntityType, TEntityType> Deduplicate { get; set; } = (previous, current) => current;

        /// <summary>
        /// The entity mapper.
        /// </summary>
        public IEntityMapper<TEntityType> Mapper { get; set; }

        /// <summary>
        /// The previous entity that was mapped using this entity mapper.
        /// </summary>
        protected TEntityType Previous { get; set; }

        /// <summary>
        /// Maps a row of data to an entity.
        /// </summary>
        /// <param name="context">A context that contains information used to map Dapper objects.</param>
        /// <returns>The mapped entity, or null if the entity has previously been returned.</returns>
        public virtual TEntityType Map(EntityMapContext<TEntityType> context)
        {
            // Deduplicate the top object (entity) in the list
            if (context.Items != null &&
                context.Items.Any())
            {
                if (Previous != null &&
                    context.Items.First() is TEntityType entity)
                {
                    entity = Deduplicate(Previous, entity);
                    if (entity == Previous)
                    {
                        context.Items = new[] { entity }.Concat(context.Items.Skip(1));
                    }
                }
            }

            // Map the object
            var next = Mapper.Map(context);

            // Return null if we are returning a duplicate object.
            // Queries can filter out null entries to prevent duplicates.
            if (object.ReferenceEquals(next, Previous))
            {
                return null;
            }

            // Save a reference to the entity
            Previous = next;

            // And, return it
            return next;
        }
    }
}
