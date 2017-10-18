using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.GraphQL
{
    public class SqlUpdateContext
    {
        public DynamicParameters Parameters { get; set; }
        private Dapper.SqlBuilder SqlBuilder { get; set; }
        public string Table { get; private set; }
        private IServiceProvider ServiceProvider { get; set; }

        public SqlUpdateContext(
            string table)
        {
            this.Parameters = new DynamicParameters();
            this.SqlBuilder = new Dapper.SqlBuilder();
            this.Table = table;
        }

        /// <summary>
        /// Adds a WHERE clause to the query, joining it with the previous with an 'AND' operator if needed.
        /// </summary>
        /// <remarks>
        /// Do not include the 'WHERE' keyword, as it is added automatically.
        /// </remarks>
        /// <example>
        ///     var queryBuilder = new SqlQueryBuilder();
        ///     queryBuilder.From("Customer customer");
        ///     queryBuilder.Select(
        ///         "customer.id",
        ///         "customer.name",
        ///     );
        ///     queryBuilder.SplitOn<Customer>("id");
        ///     queryBuilder.Where("customer.id == @id");
        ///     queryBuilder.Parameters.Add("id", 1);
        ///     var customer = queryBuilder
        ///         // Execute using the database connection, and providing the primary key
        ///         // used to split entities.
        ///         .Execute(dbConnection, customer => customer.Id);
        ///         .FirstOrDefault();
        ///
        ///     // SELECT customer.id, customer.name
        ///     // FROM Customer customer
        ///     // WHERE customer.id == @id
        /// </example>
        /// <param name="where">An array of WHERE clauses.</param>
        /// <param name="parameters">Parameters included in the statement.</param>
        /// <returns>The query builder.</returns>
        public SqlUpdateContext AndWhere(string where, dynamic parameters = null)
        {
            SqlBuilder.Where(where, parameters);
            return this;
        }

        /// <summary>
        /// Executes the update statement with Dapper, using the provided database connection.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        public async Task ExecuteAsync(IDbConnection connection)
        {
            await connection.QueryAsync(
                sql: this.ToString(),
                param: this.Parameters
            );
        }

        /// <summary>
        /// Adds a WHERE clause to the query, joining it with the previous with an 'OR' operator if needed.
        /// </summary>
        /// <remarks>
        /// Do not include the 'WHERE' keyword, as it is added automatically.
        /// </remarks>
        /// <param name="where">A WHERE clause.</param>
        /// <param name="parameters">Parameters included in the statement.</param>
        /// <returns>The query builder.</returns>
        public SqlUpdateContext OrWhere(string where, dynamic parameters = null)
        {
            SqlBuilder.OrWhere(where, parameters);
            return this;
        }

        /// <summary>
        /// Renders the generated SQL statement.
        /// </summary>
        /// <returns>The rendered SQL statement.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append($"UPDATE {Table} SET ");
            sb.Append(string.Join(", ", Parameters
                .ParameterNames
                .Select(name => $"{name} = @{name}")
            ));
            return sb.ToString();
        }

        /// <summary>
        /// An alias for AndWhere().
        /// </summary>
        /// <param name="where">A WHERE clause.</param>
        /// <param name="parameters">Parameters included in the statement.</param>
        public SqlUpdateContext Where(string where, dynamic parameters = null)
        {
            return AndWhere(where);
        }
    }
}