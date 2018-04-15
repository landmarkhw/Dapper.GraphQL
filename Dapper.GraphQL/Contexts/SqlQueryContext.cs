using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Dapper.GraphQL
{
    public class SqlQueryContext<TEntityType> : SqlQueryContext
    {
        public SqlQueryContext(string alias = null) :
            base(alias == null ? TableHelper.GetTableName<TEntityType>() : $"{TableHelper.GetTableName<TEntityType>()} {alias}")
        {
            _types.Add(typeof(TEntityType));
        }
    }

    public class SqlQueryContext
    {
        protected List<string> _splitOn;
        protected List<Type> _types;

        public DynamicParameters Parameters { get; set; }
        protected Dapper.SqlBuilder SqlBuilder { get; set; }
        protected Dapper.SqlBuilder.Template QueryTemplate { get; set; }

        public SqlQueryContext(string from, dynamic parameters = null)
        {
            _splitOn = new List<string>();
            _types = new List<Type>();
            Parameters = new DynamicParameters(parameters);
            SqlBuilder = new Dapper.SqlBuilder();

            // See https://github.com/StackExchange/Dapper/blob/master/Dapper.SqlBuilder/SqlBuilder.cs
            QueryTemplate = SqlBuilder.AddTemplate($@"SELECT
/**select**/
FROM {from}/**innerjoin**//**leftjoin**//**rightjoin**//**join**/
/**where**//**orderby**/");
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
        /// <returns>The query builder.</returns>
        public SqlQueryContext AndWhere(string where, dynamic parameters = null)
        {
            Parameters.AddDynamicParams(parameters);
            SqlBuilder.Where(where);
            return this;
        }

        /// <summary>
        /// Executes the query with Dapper, using the provided database connection and map function.
        /// </summary>
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
        ///         .Execute(dbConnection, objs => objs.OfType<Customer>())
        ///         .FirstOrDefault();
        ///
        ///     // SELECT customer.id, customer.name
        ///     // FROM Customer customer
        ///     // WHERE customer.id == @id
        /// </example>
        /// <typeparam name="TEntityType">The entity type to be mapped.</typeparam>
        /// <param name="connection">The database connection.</param>
        /// <param name="map">The dapper mapping function.</param>
        /// <returns>A list of entities returned by the query.</returns>
        public IEnumerable<TEntityType> Execute<TEntityType>(IDbConnection connection, Func<object[], TEntityType> map)
        {
            var results = connection.Query<TEntityType>(
                sql: this.ToString(),
                types: this._types.ToArray(),
                param: this.Parameters,
                map: map,
                splitOn: string.Join(",", this._splitOn)
            );
            return results.Where(e => e != null);
        }


        /// <summary>
        /// Executes the query with Dapper asynchronously, using the provided database connection and map function.
        /// </summary>
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
        ///         .Execute(dbConnection, objs => objs.OfType<Customer>())
        ///         .FirstOrDefault();
        ///
        ///     // SELECT customer.id, customer.name
        ///     // FROM Customer customer
        ///     // WHERE customer.id == @id
        /// </example>
        /// <typeparam name="TEntityType">The entity type to be mapped.</typeparam>
        /// <param name="connection">The database connection.</param>
        /// <param name="map">The dapper mapping function.</param>
        /// <returns>A list of entities returned by the query.</returns>
        public async Task<IEnumerable<TEntityType>> ExecuteAsync<TEntityType>(IDbConnection connection, Func<object[], TEntityType> map)
        {
            var results = await connection.QueryAsync<TEntityType>(
                sql: this.ToString(),
                types: this._types.ToArray(),
                param: this.Parameters,
                map: map,
                splitOn: string.Join(",", this._splitOn)
            );
            return results.Where(e => e != null);
        }

        /// <summary>
        /// Gets an array of types that are used to split objects during entity mapping.
        /// </summary>
        /// <returns></returns>
        public List<Type> GetSplitOnTypes()
        {
            return _types;
        }

        /// <summary>
        /// Performs an INNER JOIN.
        /// </summary>
        /// <remarks>
        /// Do not include the 'INNER JOIN' keywords, as they are added automatically.
        /// </remarks>
        /// <example>
        ///     var queryBuilder = new SqlQueryBuilder();
        ///     queryBuilder.From("Customer customer");
        ///     queryBuilder.InnerJoin("Account account ON customer.Id = account.CustomerId");
        ///     queryBuilder.Select(
        ///         "customer.id",
        ///         "account.id",
        ///     );
        ///     queryBuilder.SplitOn<Customer>("id");
        ///     queryBuilder.SplitOn<Account>("id");
        ///     queryBuilder.Where("customer.id == @id");
        ///     queryBuilder.Parameters.Add("id", 1);
        ///     var customer = queryBuilder
        ///         // Execute using the database connection, and providing the primary key
        ///         // used to split the primary entity.
        ///         .Execute(dbConnection, customer => customer.Id);
        ///         .FirstOrDefault();
        ///
        ///     // SELECT customer.id, account.id
        ///     // FROM
        ///     //     Customer customer INNER JOIN
        ///     //     Account account ON customer.Id = account.CustomerId
        ///     // WHERE customer.id == @id
        /// </example>
        /// <param name="join">The INNER JOIN clause.</param>
        /// <param name="parameters">Parameters included in the statement.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryContext InnerJoin(string join, dynamic parameters = null)
        {
            RemoveSingleTableQueryItems();

            Parameters.AddDynamicParams(parameters);
            SqlBuilder.InnerJoin(join);
            return this;
        }

        /// <summary>
        /// Performs a LEFT OUTER JOIN.
        /// </summary>
        /// <remarks>
        /// Do not include the 'LEFT OUTER JOIN' keywords, as they are added automatically.
        /// </remarks>
        /// <example>
        ///     var queryBuilder = new SqlQueryBuilder();
        ///     queryBuilder.From("Customer customer");
        ///     queryBuilder.LeftOuterJoin("Account account ON customer.Id = account.CustomerId");
        ///     queryBuilder.Select(
        ///         "customer.id",
        ///         "account.id",
        ///     );
        ///     queryBuilder.SplitOn<Customer>("id");
        ///     queryBuilder.SplitOn<Account>("id");
        ///     queryBuilder.Where("customer.id == @id");
        ///     queryBuilder.Parameters.Add("id", 1);
        ///     var customer = queryBuilder
        ///         // Execute using the database connection, and providing the primary key
        ///         // used to split the primary entity.
        ///         .Execute(dbConnection, customer => customer.Id);
        ///         .FirstOrDefault();
        ///
        ///     // SELECT customer.id, account.id
        ///     // FROM
        ///     //     Customer customer LEFT OUTER JOIN
        ///     //     Account account ON customer.Id = account.CustomerId
        ///     // WHERE customer.id == @id
        /// </example>
        /// <param name="join">The LEFT JOIN clause.</param>
        /// <param name="parameters">Parameters included in the statement.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryContext LeftJoin(string join, dynamic parameters = null)
        {
            RemoveSingleTableQueryItems();

            Parameters.AddDynamicParams(parameters);
            SqlBuilder.LeftJoin(join);
            return this;
        }

        /// <summary>
        /// Adds an ORDER BY clause to the end of the query.
        /// </summary>
        /// <remarks>
        /// Do not include the 'ORDER BY' keywords, as they are added automatically.
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
        ///     queryBuilder.Orderby("customer.name");
        ///     var customer = queryBuilder
        ///         // Execute using the database connection, and providing the primary key
        ///         // used to split the primary entity.
        ///         .Execute(dbConnection, customer => customer.Id);
        ///         .FirstOrDefault();
        ///
        ///     // SELECT customer.id, customer.name
        ///     // FROM Customer customer
        ///     // WHERE customer.id == @id
        ///     // ORDER BY customer.name
        /// </example>
        /// <param name="orderBy">One or more GROUP BY clauses.</param>
        /// <param name="parameters">Parameters included in the statement.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryContext OrderBy(string orderBy, dynamic parameters = null)
        {
            Parameters.AddDynamicParams(parameters);
            SqlBuilder.OrderBy(orderBy);
            return this;
        }

        /// <summary>
        /// Adds a WHERE clause to the query, joining it with the previous with an 'OR' operator if needed.
        /// </summary>
        /// <remarks>
        /// Do not include the 'WHERE' keyword, as it is added automatically.
        /// </remarks>
        /// <param name="where">An array of WHERE clauses.</param>
        /// <param name="parameters">Parameters included in the statement.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryContext OrWhere(string where, dynamic parameters = null)
        {
            Parameters.AddDynamicParams(parameters);
            SqlBuilder.OrWhere(where);
            return this;
        }

        /// <summary>
        /// Adds a SELECT statement to the query, joining it with previous items already selected.
        /// </summary>
        /// <remarks>
        /// Do not include the 'SELECT' keyword, as it is added automatically.
        /// </remarks>
        /// <example>
        ///     var queryBuilder = new SqlQueryBuilder();
        ///     var customer = queryBuilder
        ///         .From("Customer customer")
        ///         .Select(
        ///            "customer.id",
        ///            "customer.name",
        ///         )
        ///         .SplitOn<Customer>("id")
        ///         .Where("customer.id == @id")
        ///         .WithParameter("id", 1)
        ///         // Execute using the database connection, and providing the primary key
        ///         // used to split entities.
        ///         .Execute(dbConnection, customer => customer.Id);
        ///         .FirstOrDefault();
        ///
        ///     // SELECT customer.id, customer.name
        ///     // FROM Customer customer
        ///     // WHERE customer.id == @id
        /// </example>
        /// <param name="select">The column to select.</param>
        /// <param name="parameters">Parameters included in the statement.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryContext Select(string select, dynamic parameters = null)
        {
            Parameters.AddDynamicParams(parameters);
            SqlBuilder.Select(select);
            return this;
        }

        public SqlQueryContext Select(params string[] select)
        {
            foreach (var s in select)
            {
                SqlBuilder.Select(s);
            }
            return this;
        }

        /// <summary>
        /// Instructs dapper to deserialized data into a different type, beginning with the specified column.
        /// </summary>
        /// <typeparam name="TEntityType">The type to map data into.</typeparam>
        /// <param name="columnName">The name of the column to map into a different type.</param>
        /// <see cref="http://dapper-tutorial.net/result-multi-mapping" />
        /// <returns>The query builder.</returns>
        public SqlQueryContext SplitOn<TEntityType>(string columnName)
        {
            _splitOn.Add(columnName);
            _types.Add(typeof(TEntityType));

            return this;
        }

        /// <summary>
        /// Instructs dapper to deserialized data into a different type, beginning with the specified column.
        /// </summary>
        /// <param name="columnName">The name of the column to map into a different type.</param>
        /// <param name="entityType">The type to map data into.</param>
        /// <see cref="http://dapper-tutorial.net/result-multi-mapping" />
        /// <returns>The query builder.</returns>
        public SqlQueryContext SplitOn(string columnName, Type entityType)
        {
            _splitOn.Add(columnName);
            _types.Add(entityType);

            return this;
        }

        /// <summary>
        /// Renders the generated SQL statement.
        /// </summary>
        /// <returns>The rendered SQL statement.</returns>
        public override string ToString()
        {
            return QueryTemplate.RawSql;
        }

        /// <summary>
        /// An alias for AndWhere().
        /// </summary>
        /// <param name="where">The WHERE clause.</param>
        /// <param name="parameters">Parameters included in the statement.</param>
        public SqlQueryContext Where(string where, dynamic parameters = null)
        {
            return AndWhere(where, parameters);
        }

        /// <summary>
        /// Clears out items that are only relevant for single-table queries.
        /// </summary>
        private void RemoveSingleTableQueryItems()
        {
            if (_types.Count > 0 && _splitOn.Count == 0)
            {
                _types.Clear();
            }
        }
    }
}