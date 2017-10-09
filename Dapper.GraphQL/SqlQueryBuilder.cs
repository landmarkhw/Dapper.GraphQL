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
    /// A builder for SQL queries.
    /// </summary>
    public class SqlQueryBuilder
    {
        // FIXME: consider making SqlQueryBuilder immutable.

        private readonly Regex AliasPattern = new Regex(@"^\s*(\[?[\w#_$]+\]?\.)?\s*(\[?[\w#_$]+\]?\.)?\[?([\w#_$]+)\]?\s+(as\s+)?\[?(?<alias>[\w#_$]+)\]?", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        /// <summary>
        /// A list of parameters provided to the query.
        /// </summary>
        public DynamicParameters Parameters { get; set; }

        private StringBuilder _from { get; set; }
        private StringBuilder _orderBy { get; set; }
        private StringBuilder _select { get; set; }
        private List<string> _splitOn { get; set; }
        private List<Type> _types { get; set; }
        private StringBuilder _where { get; set; }
        private HashSet<string> Aliases { get; set; }
        private IServiceProvider ServiceProvider { get; set; }

        public SqlQueryBuilder()
        {
            Aliases = new HashSet<string>();
            Parameters = new DynamicParameters();
            _splitOn = new List<string>();
            _types = new List<Type>();
            _select = new StringBuilder();
            _from = new StringBuilder();
            _where = new StringBuilder();
            _orderBy = new StringBuilder();
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
        public SqlQueryBuilder AndWhere(params string[] where)
        {
            if (where.Length > 0)
            {
                if (_where.Length > 0)
                {
                    _where.Append($" AND\n    {string.Join(" AND\n    ", where)}");
                }
                else _where.Append($"\nWHERE\n    {string.Join(" AND\n    ", where)}");
            }

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
            // FIXME: log instead
            Console.WriteLine(this.ToString());

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
        /// Executes the query with Dapper, using the provided database connection and map function.
        /// </summary>
        ///
        /// <typeparam name="TEntityType">The entity type to be mapped.</typeparam>
        /// <param name="connection">The database connection.</param>
        /// <param name="selector">An expression that returns the primary key of the entity.</param>
        /// <returns>A list of entities returned by the query.</returns>
        public IEnumerable<TEntityType> Execute<TEntityType>(IDbConnection connection, Func<TEntityType, object> getPrimaryKey)
            where TEntityType : class
        {
            if (ServiceProvider == null)
            {
                throw new InvalidOperationException("Cannot use this version of 'Execute' without providing a Dependency Injection container using 'WithContainer'.");
            }

            // Get the entity mapper factory
            var mapperFactory = ServiceProvider.GetService(typeof(IEntityMapperFactory)) as IEntityMapperFactory;

            // Build a mapper for this entity
            var entityMapper = mapperFactory.Build<TEntityType>(getPrimaryKey);

            var results = connection.Query<TEntityType>(
                sql: this.ToString(),
                types: this._types.ToArray(),
                param: this.Parameters,
                map: entityMapper,
                splitOn: string.Join(",", this._splitOn)
            );
            return results.Where(e => e != null);
        }

        /// <summary>
        /// Adds a FROM statement to the query.
        /// </summary>
        /// <remarks>
        /// Do not include the 'FROM' keyword, as it is added automatically.
        /// </remarks>
        /// <example>
        ///     var queryBuilder = new SqlQueryBuilder();
        ///     queryBuilder.From("Table table");
        /// </example>
        /// <param name="from">A 'FROM' statement.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryBuilder From(string from, bool ignoreDuplicates = false)
        {
            return From(new[] { from }, ignoreDuplicates);
        }

        /// <summary>
        /// Adds a FROM statement to the query.
        /// </summary>
        /// <remarks>
        /// Do not include the 'FROM' keyword, as it is added automatically.
        /// </remarks>
        /// <example>
        ///     var queryBuilder = new SqlQueryBuilder();
        ///     queryBuilder.From(new[]
        ///     {
        ///         "FirstTable first",
        ///         "SecondTable second",
        ///     });
        /// </example>
        /// <param name="from">An array of 'FROM' statements.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryBuilder From(IEnumerable<string> from, bool ignoreDuplicates = false)
        {
            if (from != null && from.Any())
            {
                if (_from.Length == 0)
                {
                    var expr = from.FirstOrDefault();
                    if (CacheAlias(expr, ignoreDuplicates))
                    {
                        _from.Append(expr);
                    }
                    from = from.Skip(1);
                }

                foreach (var expr in from)
                {
                    if (CacheAlias(expr, ignoreDuplicates))
                    {
                        _from.Append($",\n    {expr}");
                    }
                }
            }

            return this;
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
        /// <param name="ignoreDuplicates">True if duplicate aliases should be ignored, false otherwise.  Defaults to false.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryBuilder InnerJoin(string join, bool ignoreDuplicates = false)
        {
            return InnerJoin(new[] { join }, ignoreDuplicates);
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
        /// <param name="join">A list of INNER JOIN clauses.</param>
        /// <param name="ignoreDuplicates">True if duplicate aliases should be ignored, false otherwise.  Defaults to false.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryBuilder InnerJoin(IEnumerable<string> join, bool ignoreDuplicates = false)
        {
            if (join != null && join.Any())
            {
                if (_from.Length > 0)
                {
                    foreach (var expr in join)
                    {
                        if (CacheAlias(expr, ignoreDuplicates))
                        {
                            _from.Append($" INNER JOIN\n    {expr}");
                        }
                    }
                }
                else throw new NotSupportedException("Cannot join before adding a source.");
            }

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
        /// <param name="join">The INNER JOIN clause.</param>
        /// <param name="ignoreDuplicates">True if duplicate aliases should be ignored, false otherwise.  Defaults to false.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryBuilder LeftOuterJoin(string join, bool ignoreDuplicates = false)
        {
            return LeftOuterJoin(new[] { join }, ignoreDuplicates);
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
        /// <param name="join">A list of INNER JOIN clauses.</param>
        /// <param name="ignoreDuplicates">True if duplicate aliases should be ignored, false otherwise.  Defaults to false.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryBuilder LeftOuterJoin(IEnumerable<string> join, bool ignoreDuplicates = false)
        {
            if (join != null && join.Any())
            {
                if (_from.Length > 0)
                {
                    foreach (var expr in join)
                    {
                        if (CacheAlias(expr, ignoreDuplicates))
                        {
                            _from.Append($" LEFT OUTER JOIN\n    {expr}");
                        }
                    }
                }
                else throw new NotSupportedException("Cannot join before adding a source.");
            }

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
        /// <param name="ignoreDuplicates">True if duplicate aliases should be ignored, false otherwise.  Defaults to false.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryBuilder OrderBy(params string[] orderBy)
        {
            if (orderBy.Length > 0)
            {
                if (_orderBy.Length > 0)
                {
                    _orderBy.Append($",\n    {string.Join(",\n    ", orderBy)}");
                }
                else _orderBy.Append($"\nORDER BY\n    {string.Join(",\n    ", orderBy)}");
            }

            return this;
        }

        /// <summary>
        /// Adds a WHERE clause to the query, joining it with the previous with an 'OR' operator if needed.
        /// </summary>
        /// <remarks>
        /// Do not include the 'WHERE' keyword, as it is added automatically.
        /// </remarks>
        /// <param name="where">An array of WHERE clauses.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryBuilder OrWhere(params string[] where)
        {
            if (where.Length > 0)
            {
                if (_where.Length > 0)
                {
                    _where.Append($" OR\n    {string.Join(" OR\n    ", where)}");
                }
                else _where.Append($"\nWHERE\n    {string.Join(" OR\n    ", where)}");
            }

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
        /// <param name="where">An array of WHERE clauses.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryBuilder Select(params string[] select)
        {
            if (select.Length > 0)
            {
                if (_select.Length > 0)
                {
                    _select.Append($",\n    {string.Join(",\n    ", select)}");
                }
                else _select.Append(string.Join(",\n    ", select));
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
        public SqlQueryBuilder SplitOn<TEntityType>(string columnName)
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
        public SqlQueryBuilder SplitOn(string columnName, Type entityType)
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
            if (_select.Length == 0)
            {
                throw new InvalidOperationException("No SELECT clause was specified.");
            }
            if (_from.Length == 0)
            {
                throw new InvalidOperationException("No FROM clause was specified.");
            }

            return $@"SELECT
    {_select}
FROM
    {_from}{_where}{_orderBy}";
        }

        /// <summary>
        /// An alias for AndWhere().
        /// </summary>
        public SqlQueryBuilder Where(params string[] where)
        {
            return AndWhere(where);
        }

        /// <summary>
        /// Supplies a Dependency Injection container to the query, to be used during execution or mapping.
        /// </summary>
        /// <param name="serviceProvider">The Dependency Injection container to be provided.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryBuilder WithContainer(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            return this;
        }

        /// <summary>
        /// Adds a parameter value to the query.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value of the parameter.</param>
        /// <returns>The query builder.</returns>
        public SqlQueryBuilder WithParameter<TValue>(string name, TValue value)
        {
            Parameters.Add(name, value);
            return this;
        }

        /// <summary>
        /// Caches an alias with a SQL expression, to ensure it is tracked and unduplicated.
        /// </summary>
        /// <param name="expr">The FROM or JOIN expression whose alias should be tracked.</param>
        /// <param name="ignoreDuplicates">True if duplicates should be ignored, false otherwise.</param>
        /// <returns>True if the alias hasn't been seen yet and was added to the cache, false if the alias has already been seen.</returns>
        private bool CacheAlias(string expr, bool ignoreDuplicates)
        {
            var alias = ParseAlias(expr);
            if (alias == null)
            {
                throw new InvalidOperationException($"No alias found in expression '{expr}'.");
            }
            if (Aliases.Contains(alias))
            {
                if (ignoreDuplicates)
                {
                    return false;
                }
                else
                {
                    throw new InvalidOperationException($"Alias '{alias}' in expression '{expr}' has already been included in the sql query.");
                }
            }
            Aliases.Add(alias);
            return true;
        }

        /// <summary>
        /// Parses the alias in a FROM or JOIN expression.
        /// </summary>
        /// <param name="fromOrJoin">The FROM or JOIN expression to be parsed.</param>
        /// <returns>The name of the alias, if successful.  Returns null if no alias could be found or if formatted improperly.</returns>
        private string ParseAlias(string fromOrJoin)
        {
            var match = AliasPattern.Match(fromOrJoin);
            if (match.Success)
            {
                return match.Groups["alias"].Value;
            }
            return null;
        }
    }
}