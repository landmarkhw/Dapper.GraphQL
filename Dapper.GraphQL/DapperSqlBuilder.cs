namespace Dapper.GraphQL
{
    /// <summary>
    /// A builder for SQL queries and statements inheriting the official Dapper.Sql Builder to extend its functions.
    /// </summary>
    public class DapperSqlBuilder : Dapper.SqlBuilder
    {
        /// <summary>
        /// If the object has an offset there is no need to the fetch function to add an offset with 0 rows to skip 
        /// (offset clause is a must when using the fetch clause)
        /// </summary>
        private bool _hasOffset = false;

        /// <summary>
        /// Adds an Offset clause to allow pagination (it will skip N rows)
        /// </summary>
        /// <remarks>
        /// Order by clause is a must when using offset
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
        ///     queryBuilder.Offset(20);
        ///     var customer = queryBuilder
        ///         .Execute<Customer>(dbConnection, graphQLSelectionSet);
        ///         .FirstOrDefault();
        ///
        ///     // SELECT customer.id, customer.name
        ///     // FROM Customer customer
        ///     // WHERE customer.id == @id
        ///     // ORDER BY customer.name
        /// </example>
        /// <param name="rowsToSkip">total of rows to skip</param>
        /// <returns>The query builder</returns>
        public DapperSqlBuilder Offset(int rowsToSkip)
        {
            _hasOffset = true;
            return AddClause("offset", $"{rowsToSkip}", null, " + ", "OFFSET ", " ROWS\n", false) as DapperSqlBuilder;
        }

        /// <summary>
        /// Adds a fetch clause to allow pagination
        /// </summary>
        /// <remarks>
        /// Order by clause is a must when using fetch
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
        ///     queryBuilder.Offset(20);
        ///     queryBuilder.Fetch(10);
        ///     var customer = queryBuilder
        ///         .Execute<Customer>(dbConnection, graphQLSelectionSet);
        ///         .FirstOrDefault();
        ///
        ///     // SELECT customer.id, customer.name
        ///     // FROM Customer customer
        ///     // WHERE customer.id == @id
        ///     // ORDER BY customer.name
        /// </example>
        /// <param name="rowsToReturn">total of rows to return</param>
        /// <returns>The query builder.</returns>
        public DapperSqlBuilder Fetch(int rowsToReturn)
        {
            if(!_hasOffset)
                Offset(0);
            return AddClause("fetch", $"{rowsToReturn}", null, " + ", "FETCH FIRST ", " ROWS ONLY\n", false) as DapperSqlBuilder;
        }
    }
}