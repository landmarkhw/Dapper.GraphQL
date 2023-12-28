using System;
using System.Collections.Generic;
using System.Linq;
using GraphQL.Language.AST;
using GraphQLParser.AST;

namespace Dapper.GraphQL
{
    public sealed class EntityMapContext : IDisposable
    {
        private readonly object _lockObject = new object();
        private bool _isDisposing = false;

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
        public IHasSelectionSetNode SelectionSet { get; set; }

        /// <summary>
        /// The types used to split the GraphQL query.
        /// </summary>
        public IEnumerable<Type> SplitOn { get; set; }

        protected IDictionary<GraphQLName, GraphQLField> CurrentSelectionSet { get; set; }

        protected IEnumerator<object> ItemEnumerator { get; set; }

        protected IEnumerator<Type> SplitOnEnumerator { get; set; }

        public void Dispose()
        {
            lock (_lockObject)
            {
                if (!_isDisposing)
                {
                    _isDisposing = true;

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

        /// <summary>
        /// Returns a map of selected GraphQL fields.
        /// </summary>
        public IDictionary<GraphQLName, GraphQLField> GetSelectedFields()
        {
            return SelectionSet.GetSelectedFields();
        }

        /// <summary>
        /// Maps the next object from Dapper.
        /// </summary>
        /// <param name="fieldNames">The names of one or more GraphQL fields associated with the item.</param>
        /// /// <param name="getSelectionSet">Gets information if node is selected.</param>
        /// <param name="entityMapper">An optional entity mapper.  This is used to map complex objects from Dapper mapping results.</param>
        /// <typeparam name="TItemType">The item type to be mapped.</typeparam>
        /// <returns>The mapped item.</returns>
        public TItemType Next<TItemType>(
            IEnumerable<string> fieldNames,
            Func<IDictionary<GraphQLName, GraphQLField>, IHasSelectionSetNode, IHasSelectionSetNode> getSelectionSet,
            IEntityMapper<TItemType> entityMapper = null)
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

            lock (_lockObject)
            {
                var keys = fieldNames.Intersect(CurrentSelectionSet.Keys.Select(k => k.StringValue));
                if (keys.Any())
                {
                    var item = default(TItemType);
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
                        var selectionSet = getSelectionSet(CurrentSelectionSet, SelectionSet);

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
                            var i = 0;
                            while (

                                // Less 1, the next time we iterate we
                                // will advance by 1 as part of the iteration.
                                i < mappedCount - 1 &&
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

        /// <summary>
        /// Begins mapping objects from Dapper.
        /// </summary>
        /// <typeparam name="TEntityType">The entity type to be mapped.</typeparam>
        /// <returns>The mapped entity.</returns>
        public TEntityType Start<TEntityType>()
            where TEntityType : class
        {
            lock (_lockObject)
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
    }
}
