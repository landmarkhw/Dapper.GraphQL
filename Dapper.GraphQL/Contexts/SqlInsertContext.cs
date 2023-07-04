using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.GraphQL
{
    public class SqlInsertContext<TEntityType> :
        SqlInsertContext
        where TEntityType : class
    {
        private List<SqlInsertContext<TEntityType>> Inserts { get; set; }

        public SqlInsertContext(string table, TEntityType obj)
            : base(table, obj)
        {
        }

        /// <summary>
        /// Adds an additional INSERT statement after this one.
        /// </summary>
        /// <param name="obj">The data to be inserted.</param>
        /// <returns>The context of the INSERT statement.</returns>
        public virtual SqlInsertContext Insert(TEntityType obj)
        {
            if (Inserts == null)
            {
                Inserts = new List<SqlInsertContext<TEntityType>>();
            }
            var insert = SqlBuilder.Insert(obj);
            Inserts.Add(insert);
            return this;
        }
    }

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
        /// <param name="transaction">The transaction to execute under (optional).</param>
        /// <param name="options">The options for the command (optional).</param>
        public int Execute(IDbConnection connection, IDbTransaction transaction = null, SqlMapperOptions options = null)
        {
            if (options == null) {
                options = SqlMapperOptions.DefaultOptions;
            }

            int result = connection.Execute(BuildSql(), Parameters, transaction, options.CommandTimeout, options.CommandType);
            if (Inserts != null)
            {
                // Execute each insert and aggregate the results
                result = Inserts.Aggregate(result, (current, insert) => current + insert.Execute(connection, transaction, options));
            }
            return result;
        }

        /// <summary>
        /// Executes the INSERT statements with Dapper asynchronously, using the provided database connection.
        /// </summary>
        /// <param name="connection">The database connection.</param>
        /// <param name="transaction">The transaction to execute under (optional).</param>
        /// <param name="options">The options for the command (optional).</param>
        public async Task<int> ExecuteAsync(IDbConnection connection, IDbTransaction transaction = null, SqlMapperOptions options = null)
        {
            if (options == null) {
                options = SqlMapperOptions.DefaultOptions;
            }

            int result = await connection.ExecuteAsync(BuildSql(), Parameters, transaction, options.CommandTimeout, options.CommandType);
            if (Inserts != null)
            {
                // Execute each insert and aggregate the results
                result = await Inserts.AggregateAsync(result, async (current, insert) => current + await insert.ExecuteAsync(connection, transaction, options));
            }
            return result;
        }

        /// <summary>
        /// Adds an additional INSERT statement after this one.
        /// </summary>
        /// <typeparam name="TEntityType">The type of entity to be inserted.</typeparam>
        /// <param name="obj">The data to be inserted.</param>
        /// <returns>The context of the INSERT statement.</returns>
        public virtual SqlInsertContext Insert<TEntityType>(TEntityType obj)
            where TEntityType : class
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