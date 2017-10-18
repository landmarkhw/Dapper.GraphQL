using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.GraphQL
{
    public class SqlInsertContext
    {
        private readonly string table;

        public DynamicParameters Parameters { get; private set; }

        public SqlInsertContext(
            string table,
            dynamic parameters = null)
        {
            this.table = table;
            this.Parameters = new DynamicParameters(parameters);
        }
    }
}