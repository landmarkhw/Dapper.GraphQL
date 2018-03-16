using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Dapper.GraphQL
{
    public class SqlInsertContext
    {
        private HashSet<string> InsertParameterNames;
        public DynamicParameters Parameters { get; set; }
        public string Table { get; private set; }
        private List<SqlInsertContext> Inserts { get; set; }

        public SqlInsertContext(
            string table,
            dynamic parameters = null)
        {
            if (parameters != null && !(parameters is IEnumerable<KeyValuePair<string, object>>))
            {
                parameters = ParameterHelper.GetSetFlatProperties(parameters);
            }
            this.Parameters = new DynamicParameters(parameters);
            this.InsertParameterNames = new HashSet<string>(Parameters.ParameterNames);
            this.Table = table;
        }

        /// <summary>
        /// Executes the INSERT statements with Dapper, using the provided database connection.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        public int Execute(IDbConnection connection)
        {
            int result = connection.Execute(BuildSql(), Parameters);
            if (Inserts != null)
            {
                // Execute each insert and aggregate the results
                result = Inserts.Aggregate(result, (current, insert) => current + insert.Execute(connection));
            }
            return result;
        }

        /// <summary>
        /// Adds an additional INSERT statement after this one.
        /// </summary>
        /// <typeparam name="TEntityType">The type of entity to be inserted.</typeparam>
        /// <param name="obj">The data to be inserted.</param>
        /// <returns>The context of the INSERT statement.</returns>
        public SqlInsertContext Insert<TEntityType>(TEntityType obj)
        {
            if (Inserts == null)
            {
                Inserts = new List<SqlInsertContext>();
            }
            var insert = SqlBuilder.Insert(obj);
            Inserts.Add(insert);
            return this;
        }

        /// <summary>
        /// Adds an additional INSERT statement after this one.
        /// </summary>
        /// <param name="table">The table to insert data into.</param>
        /// <param name="parameters">The data to be inserted.</param>
        /// <returns>The context of the INSERT statement.</returns>
        public SqlInsertContext Insert(string table, dynamic parameters = null)
        {
            if (Inserts == null)
            {
                Inserts = new List<SqlInsertContext>();
            }
            var insert = SqlBuilder.Insert(table, parameters);
            Inserts.Add(insert);
            return this;
        }

        /// <summary>
        /// Renders the generated SQL statement.
        /// </summary>
        /// <returns>The rendered SQL statement.</returns>
        public override string ToString()
        {
            return BuildSql();
        }

        /// <summary>
        /// Builds the INSERT statement.
        /// </summary>
        /// <returns>A SQL INSERT statement.</returns>
        private string BuildSql()
        {
            var sb = new StringBuilder();
            sb.Append($"INSERT INTO {Table} (");
            sb.Append(string.Join(", ", InsertParameterNames));
            sb.Append(") VALUES (");
            sb.Append(string.Join(", ", InsertParameterNames.Select(name => $"@{name}")));
            sb.Append(");");
            return sb.ToString();
        }
    }
}