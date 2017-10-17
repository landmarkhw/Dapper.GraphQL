using Dapper.GraphQL.Contexts;
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
        public static SqlQueryContext From(string from, bool ignoreDuplicates = false)
        {
            return new SqlQueryContext().From(from, ignoreDuplicates);
        }

        public static SqlQueryContext From(IEnumerable<string> from, bool ignoreDuplicates = false)
        {
            return new SqlQueryContext().From(from, ignoreDuplicates);
        }
    }
}