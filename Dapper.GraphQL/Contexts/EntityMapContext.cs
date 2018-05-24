using GraphQL.Language.AST;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapper.GraphQL
{
    public class EntityMapContext : IDisposable
    {
        private bool IsDisposing = false;
        private object LockObject = new object();

        protected IDictionary<string, Field> CurrentSelectionSet { get; set; }
        protected IEnumerator<object> ItemEnumerator { get; set; }
        protected IEnumerator<Type> SplitOnEnumerator { get; set; }

        /// <summary>
        /// A list of objects to be mapped.
        /// </summary>
        public IEnumerable<object> Items { get; set; }

        /// <summary>
        /// The count of objects that have been mapped.
        /// </summary>
        public int MappedCount { get; protected set; } = 0;

        /// <summary>
        /// The GraphQL selection criteria.
        /// </summary>
        public IHaveSelectionSet SelectionSet { get; set; }

        /// <summary>
        /// The types used to split the GraphQL query.
        /// </summary>
        public IEnumerable<Type> SplitOn { get; set; }

        /// <summary>
        /// Returns a map of selected GraphQL fields.
        /// </summary>
        public IDictionary<string, Field> GetSelectedFields()
        {
            return SelectionSet.GetSelectedFields();
        }

        /// <summary>
        /// Begins mapping objects from Dapper.
        /// </summary>
        /// <typeparam name="TEntityType">The entity type to be mapped.</typeparam>
        /// <returns>The mapped entity.</returns>
        public TEntityType Start<TEntityType>()
            where TEntityType : class
        {
            lock (LockObject)
            {
                ItemEnumerator = Items.GetEnumerator();
                SplitOnEnumerator = SplitOn.GetEnumerator();
                CurrentSelectionSet = SelectionSet.GetSelectedFields();
                MappedCount = 0;

                if (ItemEnumerator.MoveNext() &&
                    SplitOnEnumerator.MoveNext())
                {
                    var entity = ItemEnumerator.Current as TEntityType;
                    MappedCount++;
                    return entity;
                }
                return default(TEntityType);
            }
        }

        /// <summary>
        /// Maps the next object from Dapper.
        /// </summary>
        /// <typeparam name="TItemType">The item type to be mapped.</typeparam>
        /// <param name="context">The context used to map object from Dapper.</param>
        /// <param name="fieldNames">The names of one or more GraphQL fields associated with the item.</param>
        /// <param name="entityMapper">An optional entity mapper.  This is used to map complex objects from Dapper mapping results.</param>
        /// <returns>The mapped item.</returns>
        public TItemType Next<TItemType>(
            IEnumerable<string> fieldNames, 
            IEntityMapper<TItemType> entityMapper = null,
            Func<IDictionary<string, Field>, IHaveSelectionSet, IHaveSelectionSet> getSelectionSet = null)
            where TItemType : class
        {
            if (fieldNames == null)
            {
                throw new ArgumentNullException(nameof(fieldNames));
            }

            if (ItemEnumerator == null ||
                SplitOnEnumerator == null)
            {
                throw new NotSupportedException("Cannot call Next() before calling Start()");
            }

            lock (LockObject)
            {
                var keys = fieldNames.Intersect(CurrentSelectionSet.Keys);
                if (keys.Any())
                {
                    TItemType item = default(TItemType);
                    while (
                        ItemEnumerator.MoveNext() &&
                        SplitOnEnumerator.MoveNext())
                    {
                        // Whether a non-null object exists at this position or not,
                        // the SplitOn is expecting this type here, so we will yield it.
                        if (SplitOnEnumerator.Current == typeof(TItemType))
                        {
                            item = ItemEnumerator.Current as TItemType;
                            break;
                        }
                    }

                    if (entityMapper != null)
                    {
                        // Determine where the next entity mapper will get its selection set from
                        IHaveSelectionSet selectionSet = getSelectionSet(CurrentSelectionSet, SelectionSet);

                        var nextContext = new EntityMapContext
                        {
                            Items = Items.Skip(MappedCount),
                            SelectionSet = selectionSet,
                            SplitOn = SplitOn.Skip(MappedCount),
                        };
                        using (nextContext)
                        {
                            item = entityMapper.Map(nextContext);

                            // Update enumerators to skip past items already mapped
                            var mappedCount = nextContext.MappedCount;
                            MappedCount += nextContext.MappedCount;
                            int i = 0;
                            while (
                                i < mappedCount &&
                                ItemEnumerator.MoveNext() &&
                                SplitOnEnumerator.MoveNext())
                            {
                                i++;
                            }
                        }
                    }
                    else
                    {
                        MappedCount++;
                    }
                    return item;
                }
            }
            return default(TItemType);
        }

        public void Dispose()
        {
            lock (LockObject)
            {
                if (!IsDisposing)
                {
                    IsDisposing = true;

                    if (ItemEnumerator != null &&
                        SplitOnEnumerator != null)
                    {
                        ItemEnumerator.Dispose();
                        ItemEnumerator = null;
                        SplitOnEnumerator.Dispose();
                        SplitOnEnumerator = null;
                    }
                }
            }
        }
    }    
}
