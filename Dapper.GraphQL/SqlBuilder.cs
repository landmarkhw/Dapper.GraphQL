namespace Dapper.GraphQL
{
    /// <summary>
    /// A builder for SQL queries and statements.
    /// </summary>
    public static class SqlBuilder
    {
        public static SqlDeleteContext Delete(string from, dynamic parameters = null)
        {
            return new SqlDeleteContext(from, parameters);
        }

        public static SqlDeleteContext Delete<TEntityType>(dynamic parameters = null)
            where TEntityType : class
        {
            return new SqlDeleteContext(typeof(TEntityType).Name, parameters);
        }

        public static SqlQueryContext From(string from, dynamic parameters = null)
        {
            return new SqlQueryContext(from, parameters);
        }

        public static SqlQueryContext From<TEntityType>(string alias = null)
            where TEntityType : class
        {
            return new SqlQueryContext<TEntityType>(alias);
        }

        public static SqlInsertContext<TEntityType> Insert<TEntityType>(TEntityType obj)
            where TEntityType : class
        {
            return new SqlInsertContext<TEntityType>(typeof(TEntityType).Name, obj);
        }
        
        public static SqlInsertContext Insert(string table, dynamic parameters = null)
        {
            return new SqlInsertContext(table, parameters);
        }

        public static SqlUpdateContext Update<TEntityType>(TEntityType obj)
            where TEntityType : class
        {
            return new SqlUpdateContext(typeof(TEntityType).Name, obj);
        }

        public static SqlUpdateContext Update(string table, dynamic parameters = null)
        {
            return new SqlUpdateContext(table, parameters);
        }
    }
}