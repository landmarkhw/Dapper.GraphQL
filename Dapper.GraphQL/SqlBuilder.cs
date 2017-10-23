using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Dapper.GraphQL
{
    /// <summary>
    /// A builder for SQL queries and statements.
    /// </summary>
    public static class SqlBuilder
    {
        public static SqlQueryContext From(string from, dynamic parameters = null)
        {
            return new SqlQueryContext(from, parameters);
        }

        public static SqlQueryContext From<TEntityType>(string alias = null)
        {
            return new SqlQueryContext<TEntityType>(alias);
        }

        public static SqlInsertContext Insert<TEntityType>(TEntityType obj)
        {
            return new SqlInsertContext(typeof(TEntityType).Name, obj);
        }

        public static SqlInsertContext Insert(string table, dynamic parameters = null)
        {
            return new SqlInsertContext(table, parameters);
        }

        public static SqlUpdateContext Update<TEntityType>(TEntityType obj)
        {
            return new SqlUpdateContext(typeof(TEntityType).Name, obj);
        }

        public static SqlUpdateContext Update(string table, dynamic parameters = null)
        {
            return new SqlUpdateContext(table, parameters);
        }
    }
}