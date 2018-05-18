using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.GraphQL
{
    public class EntityMapper<TEntityType> :
        IEntityMapper<TEntityType>
        where TEntityType : class
    {
        public virtual TEntityType Map(EntityMapContext context)
        {
            var entity = context.Start<TEntityType>();
            return entity;
        }
    }
}
