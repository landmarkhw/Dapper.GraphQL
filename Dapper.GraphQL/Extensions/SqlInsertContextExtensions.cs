using System;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.GraphQL
{
    public static class SqlInsertContextExtensions
    {
        public static TIdentityType ExecuteWithSqlIdentity<TEntity, TIdentityType>(this SqlInsertContext context, IDbConnection dbConnection, Func<TEntity, TIdentityType> identityTypeSelector)
        {
            return ExecuteWithSqlIdentity<TIdentityType>(context, dbConnection);
        }

        public static TIdentityType ExecuteWithSqlIdentity<TIdentityType>(this SqlInsertContext context, IDbConnection dbConnection)
        {
            var sb = BuildSqlIdentityQuery<TIdentityType>(context);

            return dbConnection
                .Query<TIdentityType>(sb.ToString(), context.Parameters)
                .Single();
        }

        public static async Task<TIdentityType> ExecuteWithSqlIdentityAsync<TEntity, TIdentityType>(this SqlInsertContext context, IDbConnection dbConnection, Func<TEntity, TIdentityType> identityTypeSelector)
        {
            return await ExecuteWithSqlIdentityAsync<TIdentityType>(context, dbConnection);
        }

        public static async Task<TIdentityType> ExecuteWithSqlIdentityAsync<TIdentityType>(this SqlInsertContext context, IDbConnection dbConnection)
        {
            var sb = BuildSqlIdentityQuery<TIdentityType>(context);

            var task = dbConnection
                .QueryAsync<TIdentityType>(sb.ToString(), context.Parameters);
            return (await task).Single();
        }

        private static StringBuilder BuildSqlIdentityQuery<TIdentityType>(SqlInsertContext context)
        {
            var sb = new StringBuilder();

            if (typeof(TIdentityType) == typeof(int))
            {
                sb.AppendLine(context.ToString());
                sb.AppendLine("SELECT CAST(SCOPE_IDENTITY() AS INT)");
            }
            else if (typeof(TIdentityType) == typeof(long))
            {
                sb.AppendLine(context.ToString());
                sb.AppendLine("SELECT CAST(SCOPE_IDENTITY() AS BIGINT)");
            }
            else throw new InvalidCastException($"Type {typeof(TIdentityType).Name} in not supported this SQL context.");

            return sb;
        }

        public static int ExecuteWithSqliteIdentity(this SqlInsertContext context, IDbConnection dbConnection)
        {
            var sb = BuildSqliteIdentityQuery(context);

            return dbConnection
                .Query<int>(sb.ToString(), context.Parameters)
                .Single();
        }

        public static async Task<int> ExecuteWithSqliteIdentityAsync(this SqlInsertContext context, IDbConnection dbConnection)
        {
            var sb = BuildSqliteIdentityQuery(context);

            var task = dbConnection
                .QueryAsync<int>(sb.ToString(), context.Parameters);
            return (await task).Single();
        }

        private static StringBuilder BuildSqliteIdentityQuery(SqlInsertContext context)
        {
            var sb = new StringBuilder();

            sb.AppendLine(context.ToString());
            sb.AppendLine("SELECT last_insert_rowid();");

            return sb;
        }
    }
}