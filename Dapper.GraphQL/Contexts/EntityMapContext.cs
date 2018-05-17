using GraphQL.Language.AST;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Dapper.GraphQL
{
    public class EntityMapContext<TEntityType>
        where TEntityType : class
    {
        private IDictionary<string, Field> CurrentSelectionSet { get; set; }
        private IEnumerator<object> ItemEnumerator { get; set; }
        private IEnumerator<Type> SplitOnEnumerator { get; set; }

        //private int FindIndex<TItemType>(IEnumerable<TItemType> items, TItemType item)
        //{
        //    return items
        //        .Select((i, index) => new { i, index })
        //        .FirstOrDefault(i => object.Equals(i.i, item))?
        //        .index ?? -1;
        //}

        public IEnumerable<object> Items { get; set; }
        public int MappedCount { get; private set; } = 0;
        public IHaveSelectionSet SelectionSet { get; set; }
        public IEnumerable<Type> SplitOn { get; set; }

        public IDictionary<string, Field> GetSelectedFields()
        {
            return SelectionSet.GetSelectedFields();
        }

        public TEntityType Start()
        {
            ItemEnumerator = Items.GetEnumerator();
            SplitOnEnumerator = SplitOn.GetEnumerator();
            CurrentSelectionSet = SelectionSet.GetSelectedFields();

            if (ItemEnumerator.MoveNext() &&
                SplitOnEnumerator.MoveNext())
            {
                var entity = ItemEnumerator.Current as TEntityType;
                MappedCount++;
                return entity;
            }
            return default(TEntityType);
        }

        public TItemType Next<TItemType>(string name, IEntityMapper<TItemType> entityMapper = null)
            where TItemType : class
        {
            if (CurrentSelectionSet?.ContainsKey(name) == true)
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
                    var nextContext = new EntityMapContext<TItemType>
                    {
                        Items = Items.Skip(MappedCount),
                        SelectionSet = CurrentSelectionSet[name],
                        SplitOn = SplitOn.Skip(MappedCount),
                    };
                    item = entityMapper.Map(nextContext);

                    // Update enumerators to skip past items already mapped
                    var mappedCount = nextContext.MappedCount;
                    MappedCount += nextContext.MappedCount;
                    int i = 0;
                    while (i < mappedCount &&
                        ItemEnumerator.MoveNext() &&
                        SplitOnEnumerator.MoveNext())
                    {
                        i++;
                    }
                }
                else
                {
                    MappedCount++;
                }
                return item;
            }
            return default(TItemType);
        }
    }
}
