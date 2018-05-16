using GraphQL.Language.AST;
using System;
using System.Collections.Generic;
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
        /// True if duplicate entries should return null, false otherwise.
        /// </summary>
        public bool ReturnNullForDuplicates { get; set; } = true;

        /// <summary>
        /// Maps a row of data to an entity.
        /// </summary>
        /// <param name="objs">A row objects to be mapped.</param>
        /// <param name="selectionSet">The GraphQL selection set (optional).</param>
        /// <param name="splitOn">The types the query is split on.</param>
        /// <returns>The mapped entity, or null if the entity has previously been returned.</returns>
        public virtual TEntityType Map(object[] objs, IHaveSelectionSet selectionSet, List<Type> splitOn)
        {
            // Deduplicate the top object (entity) in the list
            if (objs[0] is TEntityType entity)
            {
                objs[0] = Deduplicate(Previous, entity);
            }

            // Map the object
            var next = Mapper.Map(objs, selectionSet, splitOn);

            // Return null if we are returning a duplicate object
            if (ReturnNullForDuplicates && object.ReferenceEquals(next, Previous))
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
